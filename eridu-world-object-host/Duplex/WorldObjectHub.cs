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
        List<WorldObject> _worldObjects = new List<WorldObject>();

        #region IPresenceHub Methods

        public async Task<WorldObject[]> JoinAsync(string roomName) {

            //Group can bundle many connections and it has inmemory-storage so add any type per group
            (room) = await Group.AddAsync(roomName);

            return _worldObjects.ToArray();
            //return _worldObjectStorage.AllValues.ToArray();
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

        Task IWorldObjectHub.SpawnWorldObject(WorldObject worldObject, Matrix4x4[] transforms) {
            //TODO: Check authoritative client

            //TODO: Pass along all existing world objects.
            if (_worldObjects.Contains(worldObject)) {
                Console.WriteLine("Tried to spawn an existing world object");
            }
            else {
                _worldObjects.Add(worldObject);

                Broadcast(room).OnSpawnWorldObject(worldObject, transforms);
            }
            return Task.CompletedTask;
        }

        Task IWorldObjectHub.PlayAnimation(WorldObject worldObject, string animationName) {
            if (!_worldObjects.Contains(worldObject)) {
                Console.WriteLine("Could not find a world object to play an animation on.");
            }
            else {
                Broadcast(room).OnPlayAnimation(worldObject, animationName);
            }
            return Task.CompletedTask;
        }

        Task IWorldObjectHub.DestroyWorldObject(WorldObject worldObject) {

            if (!_worldObjects.Contains(worldObject)) {
                Console.WriteLine("Couldn't find a world object to destroy");
            }
            else {
                _worldObjects.Remove(worldObject);

                Broadcast(room).OnDestroyWorldObject(worldObject);
            }
            return Task.CompletedTask;
        }

        Task IWorldObjectHub.MoveTransforms(WorldObject worldObject, Matrix4x4[] transforms) {
            if (!_worldObjects.Contains(worldObject)) {
                Console.WriteLine("Could not find a world object to move.");
            }
            else {
                Broadcast(room).OnDestroyWorldObject(worldObject);
                //BroadcastExceptSelf(room).OnMoveTransforms(worldObjectInstanceId, transforms);
            }
            return Task.CompletedTask;

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
    }
}