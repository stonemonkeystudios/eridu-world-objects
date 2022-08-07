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
        //IInMemoryStorage<WorldObject> _worldObjectStorage;

        //TODO: Get this from a persistent source like a DB
        IInMemoryStorage<WorldObjectOwner> _ownerStorage;
        IInMemoryStorage<WorldObject> _worldObjectsStorage;

        #region IWorldObjectHubCH Methods

        public async Task<WorldObject[]> JoinAsync(string roomName) {

            //Group can bundle many connections and it has inmemory-storage so add any type per group
            (room) = await Group.AddAsync(roomName);

            return _worldObjectsStorage.AllValues.ToArray();
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
                (room, _worldObjectsStorage) = await Group.AddAsync(room.GroupName, worldObject);
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

            WorldObjectOwner owner = GetExistingOwner(worldObject);

            //The object exists in the scene
            if (wo != null) {
                if(owner != null) {
                    DoReleaseOwner(worldObject, owner.OwnerUserId);
                }
                //Remove it from storate
                _worldObjectsStorage.AllValues.Remove(wo);
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
            foreach( var key in _ownerStorage.AllValues){
                if(key.WorldObjectInstanceId == worldObject.WorldObjectInstanceId) {
                    //This object is already owned
                    return false;
                }
            }

            bool found = false;
            //Does the object actually exist in the scene
            foreach(var key in _worldObjectsStorage.AllValues) {
                if(key.WorldObjectInstanceId == worldObject.WorldObjectInstanceId) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                Console.WriteLine("No matching world object found in storage.");
                return false;
            }

            //If not, take ownership
            WorldObjectOwner owner = new WorldObjectOwner();
            owner.WorldObjectInstanceId = worldObject.WorldObjectInstanceId;
            owner.OwnerUserId = playerId;
            (room, _ownerStorage) = await Group.AddAsync(room.GroupName, owner);
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
            bool found = false;
            foreach (var key in _ownerStorage.AllValues) {
                if (key.WorldObjectInstanceId == worldObject.WorldObjectInstanceId) {
                    //This object is already owned
                    found = true;
                }
            }

            if (found) {
                WorldObjectOwner owner = null;

                //Is the owner of the object the one we knew about?
                //TODO: If we are authoritative, then force remove the object
                foreach (var key in _ownerStorage.AllValues) {
                    if (key.WorldObjectInstanceId == worldObject?.WorldObjectInstanceId &&
                        key.OwnerUserId == playerId) {
                        owner = key;
                    }
                }
                if (owner != null) {
                    _ownerStorage.AllValues.Remove(owner);
                }

                BroadcastExceptSelf(room).OnReleaseOwnership(worldObject);
            }

        }

        private WorldObject GetExistingWorldObject(WorldObject worldObject) {
            //Does the object actually exist in the scene
            foreach (var key in _worldObjectsStorage.AllValues) {
                if (key.WorldObjectInstanceId == worldObject.WorldObjectInstanceId) {
                    return key;
                }
            }
            return null;
        }

        private WorldObjectOwner GetExistingOwner(WorldObject worldObject) {
            foreach (var key in _ownerStorage.AllValues) {
                if (key.WorldObjectInstanceId == worldObject?.WorldObjectInstanceId) {
                    return key;
                }
            }
            return null;
        }
        #endregion
    }
}