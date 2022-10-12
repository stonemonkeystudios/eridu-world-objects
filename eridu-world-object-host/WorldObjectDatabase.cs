using System.Collections.Generic;

namespace Eridu.WorldObjects
{
    public class WorldObjectDatabase {
        public Dictionary<string, WorldObjectRoom> worldObjectRooms = new Dictionary<string, WorldObjectRoom>();

        private static WorldObjectDatabase? _instance;
        public static WorldObjectDatabase? Instance {
            get {
                return _instance;
            }
        }

        public WorldObjectRoom GetOrAddRoom(string roomName) {
            if(!worldObjectRooms.ContainsKey(roomName))
                worldObjectRooms.Add(roomName, new WorldObjectRoom());
            return worldObjectRooms[roomName];
        }

        public void RemoveWorldObjectRoom(string roomName) {
            if (worldObjectRooms.ContainsKey(roomName))
                worldObjectRooms.Remove(roomName);
        }

        public static void CreateInstance() {
            _instance = new WorldObjectDatabase();
        }

        public static void ClearInstance() {
            _instance = null;
        }
    }

    public class WorldObjectRoom {
        public int worldObjectId = 0;
        private Dictionary<int, WorldObject> worldObjects = new Dictionary<int, WorldObject>();

        public void AddOrUpdate(WorldObject worldObject) {
            if (worldObjects.ContainsKey(worldObject.InstanceId)) {
                worldObjects[worldObject.InstanceId] = worldObject;
            }
            else {
                worldObjects.Add(worldObject.InstanceId, worldObject);
            }
        }

        public WorldObject GetWorldObjectForId(int id) {
            if (!worldObjects.ContainsKey(id)) {
                return null;
            }
            return worldObjects[id];
        }

        public WorldObject[] GetAllWorldObjects() {
            return worldObjects.Values.ToArray();
        }

        public void ClearAllWorldObjects() {
            worldObjects.Clear();
        }

        public void Remove(WorldObject worldObject) {
            if (!worldObjects.ContainsKey(worldObject.InstanceId)) {
                Console.WriteLine("Tried removing world object that does not exist.");
            }
            else {
                worldObjects.Remove(worldObject.InstanceId);
            }
        }

        public int GetAndIncrementNextId() {
            int woId = worldObjectId;
            worldObjectId++;
            return woId;
        }
    }
}
