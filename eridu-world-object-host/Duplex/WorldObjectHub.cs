using System;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using UnityEngine;
using System.Linq;
using Eridu.Common;

namespace Eridu.WorldObjects {
    // Implements RPC service in the server project.
    // The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
    [GroupConfiguration(typeof(ConcurrentDictionaryGroupRepositoryFactory))]
    public class WorldObjectHub : StreamingHubBase<IWorldObjectHub, IWorldObjectHubReceiver>, IWorldObjectHub {
        IGroup room;
        EriduPlayer self;

        List<WorldObject> _worldObjectsStorage = new List<WorldObject>();
        Dictionary<WorldObject, int> ownedObjects = new Dictionary<WorldObject, int>();
        IInMemoryStorage<EriduPlayer> _clientStorage;

        #region IWorldObjectHubCH Methods

        public async Task<WorldObject[]> JoinAsync(string roomName, EriduPlayer player) {
            self = player;

            //Group can bundle many connections and it has inmemory-storage so add any type per group
            (room, _clientStorage) = await Group.AddAsync(roomName, self);

            var worldObjects = WorldObjectDatabase.Instance?.GetOrAddRoom(room.GroupName).GetAllWorldObjects();

            Broadcast(room).OnJoin(worldObjects);
            return worldObjects;
        }

        public async Task LeaveAsync() {
            if (room != null) {
                await room.RemoveAsync(this.Context);
            }
        }

        protected override ValueTask OnDisconnected() {
            if (self != null && self.IsRoot) {
                ClearAndDestroyAllWorldObjects();
            }
            return CompletedTask;
        }

        protected override ValueTask OnConnecting() {
            return CompletedTask;
        }

        public async Task<WorldObject> SpawnWorldObject(WorldObject worldObject, Matrix4x4 transforms) {
            var obj = new object();
            //TODO: Check authoritative client

            //Are we already tracking this id?
            worldObject.InstanceId = WorldObjectDatabase.Instance.GetOrAddRoom(room.GroupName).GetAndIncrementNextId();
            worldObject.LastRootPosition = transforms;
            WorldObjectDatabase.Instance.GetOrAddRoom(room.GroupName).AddOrUpdate(worldObject);

            Broadcast(room).OnSpawnWorldObject(worldObject, transforms);
            return worldObject;
        }

        Task IWorldObjectHub.ToggleWorldObject(WorldObject worldObject, bool enabled) {
            /*var wo = GetExistingWorldObject(worldObject);
            if(wo == null) {
                Console.WriteLine("Could not find a world object to toggle.");
            }
            else {
                Broadcast(room).OnToggleWorldObjectVisibility(worldObject, enabled);
            }*/
            return Task.CompletedTask;
        }

        Task IWorldObjectHub.PlayAnimation(WorldObject worldObject, string animationName) {

            /*var wo = GetExistingWorldObject(worldObject);
            if(wo == null) {
                Console.WriteLine("Could not find a world object to play an animation on.");
            }
            else {
                BroadcastExceptSelf(room).OnPlayAnimation(worldObject, animationName);
            }*/
            return Task.CompletedTask;
        }

        Task IWorldObjectHub.DestroyWorldObject(WorldObject worldObject) {
            var wo = WorldObjectDatabase.Instance.GetOrAddRoom(room.GroupName).GetWorldObjectForId(worldObject.InstanceId);

            //The object exists in the scene
            if (wo != null) {
                if(wo.Owned) {
                    DoReleaseOwner(worldObject, worldObject.OwnerId);
                }
                //Remove it from storate
                WorldObjectDatabase.Instance.GetOrAddRoom(room.GroupName).Remove(worldObject);




                Broadcast(room).OnDestroyWorldObject(worldObject);
            }
            return Task.CompletedTask;
        }

        Task IWorldObjectHub.MoveTransforms(WorldObject worldObject, Matrix4x4[] transforms) {
            //If authoritative, allow all moves

            //If owned by a client, make sure we're the client that owns it before moving


            var wo = WorldObjectDatabase.Instance.GetOrAddRoom(room.GroupName).GetWorldObjectForId(worldObject.InstanceId);
            if(wo == null) {
                return Task.CompletedTask;
            }
            wo.LastRootPosition = transforms[0];
            WorldObjectDatabase.Instance.GetOrAddRoom(room.GroupName).AddOrUpdate(wo);
            if (wo == null){
                Console.WriteLine("Could not find a world object to move.");
            }
            else {
                //Broadcast(room).OnDestroyWorldObject(worldObject);
                BroadcastExceptSelf(room).OnMoveTransforms(worldObject, transforms);
            }
            return Task.CompletedTask;

        }
        async Task<bool> IWorldObjectHub.TakeOwnership(WorldObject worldObject, int playerId) {

            /*var existingObject = WorldObjectDatabase.Instance.GetWorldObjectForId(worldObject.InstanceId);

            if (existingObject == null) {
                Console.WriteLine("No matching world object found in storage.");
                return false;
            }

            //If not, take ownership
            worldObject.Owned = true;
            worldObject.OwnerId = playerId;
            Broadcast(room).OnTakeOwnership(worldObject, playerId);*/
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

        async void ClearAndDestroyAllWorldObjects() {
            foreach(var wo in WorldObjectDatabase.Instance.GetOrAddRoom(room.GroupName).GetAllWorldObjects()) {
                await (this as IWorldObjectHub).DestroyWorldObject(wo);
            }
            WorldObjectDatabase.Instance.RemoveWorldObjectRoom(room.GroupName);
        }

        async void DoReleaseOwner(WorldObject worldObject, int playerId) {
            /*foreach (var key in _worldObjectsStorage) {
                if (key.InstanceId == worldObject.InstanceId){
                    if(key.Owned && key.OwnerId == playerId) {
                        //This object is owned by the requesting player
                        key.OwnerId = -1;
                        key.Owned = false;
                        BroadcastExceptSelf(room).OnReleaseOwnership(worldObject);
                        return;
                    }
                }
            }*/
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