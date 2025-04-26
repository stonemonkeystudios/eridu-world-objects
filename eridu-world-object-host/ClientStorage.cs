using System;
using System.Collections.Generic;

public class ClientStorage {
    private Dictionary<int, Guid> _clientDictionary = new Dictionary<int, Guid>();
    private static ClientStorage _instance = null;

    public static ClientStorage Instance {
        get
        {
            if (_instance == null)
                _instance = new ClientStorage();
            return _instance;
        }
    }

    public void AddClient(int clientId, Guid connectionId) {
        if (_clientDictionary.ContainsKey(clientId)) {
            _clientDictionary.Remove(clientId);
        }
        _clientDictionary.Add(clientId, connectionId);
    }

    public void RemoveClient(int clientId) {
        if (_clientDictionary.ContainsKey(clientId)) {
            _clientDictionary.Remove(clientId);
        }
    }

    public Guid? GetConnectionIdFromClientId(int clientId) {
        if (_clientDictionary.ContainsKey(clientId)) {
            return _clientDictionary[clientId];
        }
        return null;
    }
}
