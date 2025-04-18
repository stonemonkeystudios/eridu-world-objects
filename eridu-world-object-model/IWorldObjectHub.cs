﻿using System;
using System.Threading.Tasks;
using MagicOnion;
using UnityEngine;
using MessagePack;
using HQDotNet;
using Eridu.Common;

namespace Eridu.WorldObjects
{
    public interface IWorldObjectHubReceiver : IDispatchListener
    {
        // return type should be `void` or `Task`, parameters are free.
        void OnJoin(WorldObject[] existingObjects);
        void OnLeave();
        void OnSpawnWorldObject(WorldObject worldObject, Matrix4x4 transform);
        void OnReceivedData(int instanceId, string dataType, string data);
        void OnToggleWorldObjectVisibility(WorldObject worldObject, bool enabled);
        void OnPlayAnimation(WorldObject worldObject, string animationName);
        void OnDestroyWorldObject(WorldObject worldObject);
        void OnMoveTransforms(WorldObject worldObject, Matrix4x4[] transforms);
        void OnTakeOwnership(WorldObject worldObject, int playerId);
        void OnReleaseOwnership(WorldObject worldObject);
    }

    public interface IWorldObjectHub : IStreamingHub<IWorldObjectHub, IWorldObjectHubReceiver> {
        // return type should be `Task` or `Task<T>`, parameters are free.
        Task<WorldObject[]> JoinAsync(string roomName, EriduPlayer player  );
        Task<WorldObject> SpawnWorldObject(WorldObject worldObject, Matrix4x4 transforms);
        Task<WorldObject[]> GetAllWorldObjectsInRoom();
        Task ToggleWorldObject(WorldObject worldObject, bool enabled);
        Task SendData(int instanceId, string dataType, string data);
        Task PlayAnimation(WorldObject worldObject, string animationName);
        Task DestroyWorldObject(WorldObject worldObject);
        Task MoveTransforms(WorldObject worldObject, Matrix4x4[] transforms);
        Task<bool> TakeOwnership(WorldObject worldObject, int playerId);
        Task ReleaseOwnership(WorldObject worldObject, int playerId);
        Task LeaveAsync();
    }

    [MessagePackObject]
    public class WorldObject {
        [Key(0)]
        public string DbId { get; set; }
        [Key(1)]
        public int InstanceId { get; set; }
        [Key(2)]
        public int OwnerId { get; set; }
        [Key(3)]
        public bool Owned { get; set; }
        [Key(4)]
        public bool Enabled { get; set; }
        [Key(5)]
        public Matrix4x4 LastRootPosition { get; set; }
    }
}
