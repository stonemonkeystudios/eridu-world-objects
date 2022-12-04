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
        private class EriduData {
            public string dataType;
            public string data;
        }
        public int worldObjectId = 1;
        private Dictionary<int, WorldObject> worldObjects = new Dictionary<int, WorldObject>();
        private Dictionary<int, Dictionary<string, string>> worldObjectData = new Dictionary<int, Dictionary<string, string>>();

        public void AddOrUpdateData(int instanceId, string dataType, string data) {
            if (!worldObjectData.ContainsKey(instanceId)) {
                worldObjectData.Add(instanceId, new Dictionary<string, string>());
            }
            if (worldObjectData[instanceId].ContainsKey(dataType)) {
                worldObjectData[instanceId][dataType] = data;
            }
            else {
                worldObjectData[instanceId].Add(dataType, data);
            }
        }

        public WorldObjectData[] GetAllWorldObjectData() {
            List<WorldObjectData> worldObjectDatas = new List<WorldObjectData>();
            foreach (var instanceid in worldObjectData.Keys) {
                foreach(var dataType in worldObjectData[instanceid].Keys) {
                    worldObjectDatas.Add(new WorldObjectData() { instanceId = instanceid, dataType = dataType, data = worldObjectData[instanceid][dataType] });
                }
            }
            return worldObjectDatas.ToArray();
        }

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
            worldObjectData.Clear();
        }

        public void Remove(WorldObject worldObject) {
            if (!worldObjects.ContainsKey(worldObject.InstanceId)) {
                Console.WriteLine("Tried removing world object that does not exist.");
            }
            else {
                worldObjects.Remove(worldObject.InstanceId);
            }
            if(worldObjectData.ContainsKey(worldObject.InstanceId)) {
                worldObjectData.Remove(worldObject.InstanceId);
            }
        }

        public int GetAndIncrementNextId() {
            int woId = worldObjectId;
            worldObjectId++;
            return woId;
        }
    }
}
