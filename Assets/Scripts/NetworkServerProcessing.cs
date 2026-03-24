using System.Collections.Generic;
using UnityEngine;

static public class NetworkServerProcessing
{
    static int lastCreatedUniqueID = 0;

    static Dictionary<int, ClientAccountInfo> clientAccountInfoDictionary;

    const string ClientAccountFileName = "ClientAccounts.txt";
    const string LastCreatedUniqueAccountIDFileName = "LastCreatedUniqueAccountID.txt";


    #region Send and Receive Data Functions
    static public void ReceivedMessageFromClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log("Network msg received =  " + msg + ", from connection id = " + clientConnectionID + ", from pipeline = " + pipeline);

        string[] csv = msg.Split(',');
        ClientToServerSignal signal = (ClientToServerSignal)int.Parse(csv[0]);

        if (signal == ClientToServerSignal.AccountLogin)
        {
            string name = csv[1];
            string pass = csv[2];

            bool hasNameBeenFound = false;

            foreach (KeyValuePair<int, ClientAccountInfo> keyValuePair in clientAccountInfoDictionary)
            {
                ClientAccountInfo acc = keyValuePair.Value;

                if (acc.name == name)
                {
                    hasNameBeenFound = true;

                    if (acc.password == pass)
                    {
                        string toSend = ((int)ServerToClientSignal.AccountLoginSuccess).ToString();
                        SendMessageToClient(toSend, clientConnectionID, TransportPipeline.ReliableAndInOrder);
                    }
                    else
                    {
                        string toSend = ((int)ServerToClientSignal.AccountLoginPasswordError).ToString();
                        SendMessageToClient(toSend, clientConnectionID, TransportPipeline.ReliableAndInOrder);
                    }
                }
            }

            if (!hasNameBeenFound)
            {
                string toSend = ((int)ServerToClientSignal.AccountLoginUserNameError).ToString();
                SendMessageToClient(toSend, clientConnectionID, TransportPipeline.ReliableAndInOrder);
            }

        }
        else if (signal == ClientToServerSignal.AccountCreate)
        {
            string name = csv[1];
            string pass = csv[2];

            bool isUniqueUserName = true;

            foreach (KeyValuePair<int, ClientAccountInfo> keyValuePair in clientAccountInfoDictionary)
            {
                ClientAccountInfo acc = keyValuePair.Value;

                if (acc.name == name)
                {
                    isUniqueUserName = false;
                    string toSend = ((int)ServerToClientSignal.AccountCreationUserNameError).ToString();
                    SendMessageToClient(toSend, clientConnectionID, TransportPipeline.ReliableAndInOrder);
                    break;
                }
            }

            if (isUniqueUserName)
            {
                ClientAccountInfo cai = new ClientAccountInfo(name, pass);

                clientAccountInfoDictionary.Add(cai.uniqueID, cai);

                #region Serialize Data and Write to HD

                LinkedList<string> serializedData = new LinkedList<string>();

                foreach (KeyValuePair<int, ClientAccountInfo> keyValuePair in clientAccountInfoDictionary)
                {
                    ClientAccountInfo acc = keyValuePair.Value;
                    string serializedClientAccountInfo = acc.uniqueID + Utilities.Delineator + acc.name + Utilities.Delineator + acc.password;
                    serializedData.AddLast(serializedClientAccountInfo);
                }

                Utilities.WriteSerializationToHD(ClientAccountFileName, serializedData);

                #endregion

                string toSend = ((int)ServerToClientSignal.AccountCreationSuccess).ToString();
                SendMessageToClient(toSend, clientConnectionID, TransportPipeline.ReliableAndInOrder);
            }
        }
    }
    static public void SendMessageToClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient(msg, clientConnectionID, pipeline);
    }

    #endregion

    #region Connection Events

    static public void ConnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client connection, ID == " + clientConnectionID);
    }
    static public void DisconnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client disconnection, ID == " + clientConnectionID);
    }

    #endregion

    #region Setup
    static NetworkServer networkServer;
    static GameLogic gameLogic;

    static public void SetNetworkServer(NetworkServer NetworkServer)
    {
        networkServer = NetworkServer;
    }
    static public NetworkServer GetNetworkServer()
    {
        return networkServer;
    }
    static public void SetGameLogic(GameLogic GameLogic)
    {
        gameLogic = GameLogic;
    }

    static public void Init()
    {
        LinkedList<string> lastUniqueAccountIDSerializedData = Utilities.ReadSerializationFromHD(LastCreatedUniqueAccountIDFileName);
        if(lastUniqueAccountIDSerializedData.Count > 0)
            lastCreatedUniqueID = int.Parse(lastUniqueAccountIDSerializedData.First.Value);

        clientAccountInfoDictionary = new Dictionary<int, ClientAccountInfo>();
        LinkedList<string> clientAccountsSerializedData = Utilities.ReadSerializationFromHD(ClientAccountFileName);

        foreach (string s in clientAccountsSerializedData)
        {
            string[] csv = s.Split(Utilities.Delineator);
            ClientAccountInfo cai = new ClientAccountInfo(int.Parse(csv[0]), csv[1], csv[2]);
            clientAccountInfoDictionary.Add(cai.uniqueID, cai);
        }
    }

    #endregion

    static public int GenerateUniqueAccountID()
    {
        lastCreatedUniqueID++;

        LinkedList<string> serializedData = new LinkedList<string>();
        serializedData.AddLast(lastCreatedUniqueID.ToString());
        Utilities.WriteSerializationToHD(LastCreatedUniqueAccountIDFileName, serializedData);

        return lastCreatedUniqueID;
    }

}

public class ClientAccountInfo
{
    public string name;
    public string password;
    public int uniqueID;

    public ClientAccountInfo(string name, string password)
    {
        this.name = name;
        this.password = password;
        uniqueID = NetworkServerProcessing.GenerateUniqueAccountID();
    }

    public ClientAccountInfo(int uniqueID, string name, string password)
    {
        this.name = name;
        this.password = password;
        this.uniqueID = uniqueID;
    }
}

