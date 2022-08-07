using System;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using UnityEngine;
using System.Linq;

namespace Eridu.WorldObjects {
    // Implements RPC service in the server project.
    // The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
    [GroupConfiguration(typeof(ConcurrentDictionaryGroupRepositoryFactory))]
    public class WorldObjectHub : StreamingHubBase<IWorldObjectHub, IWorldObjectHubReceiver>, IWorldObjectHub {
        IGroup room;

        List<WorldObject> _worldObjectsStorage = new List<WorldObject>();

        #region IWorldObjectHubCH Methods

        public async Task<WorldObject[]> JoinAsync(string roomName) {

            //Group can bundle many connections and it has inmemory-storage so add any type per group
            (room) = await Group.AddAsync(roomName);
            
            return _worldObjectsStorage.ToArray();
        }

        public async Task LeaveAsync() {
            if (room != null) {
                await room.RemoveAsync(this.Context);
            }
        }

        protected override ValueTask OnDisconnected() {
            return CompletedTask;
        }

        protected override ValueTask OnConnecting() {
            return CompletedTask;
        }

        async Task IWorldObjectHub.SpawnWorldObject(WorldObject worldObject, Matrix4x4[] transforms) {
            //TODO: Check authoritative client

            //Are we already tracking this id?
            var wo = GetExistingWorldObject(worldObject);

            if (wo != null) {
                Console.WriteLine("Tried to spawn an existing world object");
            }
            else {
                _worldObjectsStorage.Add(worldObject);
                BroadcastExceptSelf(room).OnSpawnWorldObject(worldObject, transforms);
            }
        }

        Task IWorldObjectHub.PlayAnimation(WorldObject worldObject, string animationName) {

            var wo = GetExistingWorldObject(worldObject);
            if(wo == null) {
                Console.WriteLine("Could not find a world object to play an animation on.");
            }
            else {
                BroadcastExceptSelf(room).OnPlayAnimation(worldObject, animationName);
            }
            return Task.CompletedTask;
        }

        Task IWorldObjectHub.DestroyWorldObject(WorldObject worldObject) {

            var wo = GetExistingWorldObject(worldObject);

            //The object exists in the scene
            if (wo != null) {
                if(wo.Owned != null) {
                    DoReleaseOwner(worldObject, worldObject.OwnerId);
                }
                //Remove it from storate
                _worldObjectsStorage.Remove(worldObject);
                BroadcastExceptSelf(room).OnDestroyWorldObject(worldObject);
            }
            return Task.CompletedTask;
        }

        Task IWorldObjectHub.MoveTransforms(WorldObject worldObject, Matrix4x4[] transforms) {
            var wo = GetExistingWorldObject(worldObject);
            if(wo == null){
                Console.WriteLine("Could not find a world object to move.");
            }
            else {
                //Broadcast(room).OnDestroyWorldObject(worldObject);
                BroadcastExceptSelf(room).OnMoveTransforms(worldObject, transforms);
            }
            return Task.CompletedTask;

        }
        async Task<bool> IWorldObjectHub.TakeOwnership(WorldObject worldObject, int playerId) {

            //Does an owner exist already?
            foreach( var key in _worldObjectsStorage){
                if(key.InstanceId == worldObject.InstanceId && worldObject.Owned) {
                    //This object is already owned
                    return false;
                }
            }

            bool found = false;
            //Does the object actually exist in the scene
            foreach(var key in _worldObjectsStorage) {
                if(key.InstanceId == worldObject.InstanceId) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                Console.WriteLine("No matching world object found in storage.");
                return false;
            }

            //If not, take ownership
            worldObject.Owned = true;
            worldObject.OwnerId = playerId;
            Broadcast(room).OnTakeOwnership(worldObject, playerId);
            return true;
        }

        async Task IWorldObjectHub.ReleaseOwnership(WorldObject worldObject, int playerId) {
            DoReleaseOwner(worldObject, playerId);
        }

        public IWorldObjectHub FireAndForget() {
            return this;
        }

        public Task DisposeAsync() {
            return Task.CompletedTask;
        }

        public Task WaitForDisconnect() {
            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        async void DoReleaseOwner(WorldObject worldObject, int playerId) {
            foreach (var key in _worldObjectsStorage) {
                if (key.InstanceId == worldObject.InstanceId){
                    if(key.Owned && key.OwnerId == playerId) {
                        //This object is owned by the requesting player
                        key.OwnerId = -1;
                        key.Owned = false;
                        BroadcastExceptSelf(room).OnReleaseOwnership(worldObject);
                        return;
                    }
                }
            }
        }

        private WorldObject GetExistingWorldObject(WorldObject worldObject) {
            //Does the object actually exist in the scene
            foreach (var key in _worldObjectsStorage) {
                if (key.InstanceId == worldObject.InstanceId) {
                    return key;
                }
            }
            return null;
        }
        #endregion
    }
}