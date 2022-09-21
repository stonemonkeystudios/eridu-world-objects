using System.Collections.Generic;

namespace Eridu.WorldObjects
{
    public class WorldObjectDatabase {
        public int worldObjectId = 0;
        private Dictionary<int, WorldObject> worldObjects = new Dictionary<int, WorldObject>();

        private static WorldObjectDatabase? _instance;
        public static WorldObjectDatabase? Instance {
            get {
                return _instance;
            }
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

        public static void CreateInstance() {
            _instance = new WorldObjectDatabase();
        }

        public static void ClearInstance() {
            _instance = null;
        }
    }
}
