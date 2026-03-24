using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;

static public class NetworkServerProcessing
{
    static int lastCreatedUniqueID = 0;

    static Dictionary<int, ClientAccountInfo> clientAccountInfoDictionary;

    #region Send and Receive Data Functions
    static public void ReceivedMessageFromClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log("Network msg received =  " + msg + ", from connection id = " + clientConnectionID + ", from pipeline = " + pipeline);

        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        if (signifier == 0)
        {
            string name = csv[1];
            string pass = csv[2];

            Debug.Log("name == " + name + ", pass == " + pass);

            bool hasNameBeenFound = false;

            foreach (KeyValuePair<int, ClientAccountInfo> keyValuePair in clientAccountInfoDictionary)
            {
                ClientAccountInfo acc = keyValuePair.Value;

                if (acc.name == name)
                {
                    hasNameBeenFound = true;

                    if(acc.password == pass)
                    {
                        //login
                         SendMessageToClient("1", clientConnectionID, TransportPipeline.ReliableAndInOrder);
                    }
                    else
                    {
                        SendMessageToClient("0,Error! Password incorrect", clientConnectionID, TransportPipeline.ReliableAndInOrder);
                    }



                    // isUniqueUserName = false;
                    // //send msg to client
                    // SendMessageToClient("0,Error! User name has already been taken", clientConnectionID, TransportPipeline.ReliableAndInOrder);
                    // break;
                }

            }

            if (!hasNameBeenFound)
            {
                SendMessageToClient("0,Error! User does not exist", clientConnectionID, TransportPipeline.ReliableAndInOrder);
            }

        }
        else if (signifier == 1)
        {
            string name = csv[1];
            string pass = csv[2];

            Debug.Log("name == " + name + ", pass == " + pass);

            bool isUniqueUserName = true;

            foreach (KeyValuePair<int, ClientAccountInfo> keyValuePair in clientAccountInfoDictionary)
            {
                ClientAccountInfo acc = keyValuePair.Value;

                if (acc.name == name)
                {
                    isUniqueUserName = false;
                    //send msg to client
                    SendMessageToClient("0,Error! User name has already been taken", clientConnectionID, TransportPipeline.ReliableAndInOrder);
                    break;
                }
            }


            if (isUniqueUserName)
            {
                ClientAccountInfo cai = new ClientAccountInfo(name, pass);

                clientAccountInfoDictionary.Add(cai.uniqueID, cai);

                StreamWriter streamWriter = new StreamWriter("ClientAccounts.txt");

                foreach (KeyValuePair<int, ClientAccountInfo> keyValuePair in clientAccountInfoDictionary)
                {
                    ClientAccountInfo acc = keyValuePair.Value;
                    string serializedClientAccountInfo = acc.uniqueID + "," + acc.name + "," + acc.password;
                    streamWriter.WriteLine(serializedClientAccountInfo);
                }

                streamWriter.Close();

                SendMessageToClient("2", clientConnectionID, TransportPipeline.ReliableAndInOrder);
            }



            //create class to store account info
            //package in a data structure
            //create a unique ID
            //use a dictionary
            //save to HD
            //load account info file into dictionary
            //save/load lastCreatedUniqueID ID
            //
            //on account create: check if name is unique
            //make login work
            //clean code
            //


        }
        // else if (signifier == ClientToServerSignifiers.asd)
        // {

        // }

        //gameLogic.DoSomething();
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
        StreamReader streamReader;

        if (File.Exists("LastCreatedUniqueAccountID.txt"))
        {
            streamReader = new StreamReader("LastCreatedUniqueAccountID.txt");
            string lastCreateUniqueIDLoadedFromHD = streamReader.ReadLine();
            lastCreatedUniqueID = int.Parse(lastCreateUniqueIDLoadedFromHD);
            streamReader.Close();
        }

        clientAccountInfoDictionary = new Dictionary<int, ClientAccountInfo>();

        if (File.Exists("ClientAccounts.txt"))
        {
            streamReader = new StreamReader("ClientAccounts.txt");

            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                string[] csv = line.Split(",");

                ClientAccountInfo cai = new ClientAccountInfo(int.Parse(csv[0]), csv[1], csv[2]);

                clientAccountInfoDictionary.Add(cai.uniqueID, cai);
            }

            streamReader.Close();
        }
    }

    #endregion

    static public int GenerateUniqueAccountID()
    {
        lastCreatedUniqueID++;

        StreamWriter streamWriter = new StreamWriter("LastCreatedUniqueAccountID.txt");
        streamWriter.WriteLine(lastCreatedUniqueID);
        streamWriter.Close();

        return lastCreatedUniqueID;
    }

}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int asd = 1;
}

static public class ServerToClientSignifiers
{
    public const int asd = 1;
}

#endregion


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
















// public enum SaveDataIdentifier
//     {
//         Stats,
//         Equipment
//     }

//     const string Delineator = ",";
//     const string SaveFileName = "SavePartyData.txt";

//     static public void SavePartyButtonPressed()
//     {
//         LinkedList<string> serializedPartyData = SerializePartyData();

//         #region Save Serialized Party Data to HD

//         StreamWriter streamWriter = new StreamWriter(SaveFileName);

//         foreach (string spd in serializedPartyData)
//         {
//             streamWriter.WriteLine(spd);
//         }

//         streamWriter.Close();

//         #endregion

//     }

//     static public LinkedList<string> SerializePartyData()
//     {
//         string line;

//         LinkedList<string> serializedPartyData = new LinkedList<string>();

//         foreach (PartyCharacter pc in GameContent.partyCharacters)
//         {
//             line = Concatenate(SaveDataIdentifier.Stats, pc.classID, pc.health, pc.mana, pc.strength, pc.agility, pc.wisdom);

//             serializedPartyData.AddLast(line);
//             foreach (int e in pc.equipment)
//             {
//                 line = Concatenate(SaveDataIdentifier.Equipment, e);
//                 serializedPartyData.AddLast(line);
//             }
//         }

//         return serializedPartyData;
//     }

//     static public string Concatenate(SaveDataIdentifier identifier, params int[] values)
//     {
//         string concatenatedLine = ((int)identifier).ToString();

//         foreach (int v in values)
//         {
//             concatenatedLine = concatenatedLine + Delineator + v.ToString();
//         }

//         return concatenatedLine;
//     }

//     static public void LoadPartyButtonPressed()
//     {
//         GameContent.partyCharacters.Clear();

//         LinkedList<string> serializedPartyData = new LinkedList<string>();

//         #region Read Party Data From HD

//         StreamReader streamReader = new StreamReader(SaveFileName);

//         while (!streamReader.EndOfStream)
//         {
//             string line = streamReader.ReadLine();
//             serializedPartyData.AddLast(line);
//         }

//         streamReader.Close();

//         #endregion

//         DeserializePartyData(serializedPartyData);

//         GameContent.RefreshUI();
//     }

//     static public void DeserializePartyData(LinkedList<string> serializedPartyData)
//     {
//         LinkedList<int[]> parsedAndConvertedCSV = new LinkedList<int[]>();

//         #region Parse and Convert CSV

//         foreach (string line in serializedPartyData)
//         {
//             string[] csv = line.Split(Delineator);
//             int[] csvAsInts = new int[csv.Length];

//             for (int i = 0; i < csv.Length; i++)
//             {
//                 csvAsInts[i] = int.Parse(csv[i]);
//             }

//             parsedAndConvertedCSV.AddLast(csvAsInts);
//         }

//         #endregion

//         foreach (int[] pData in parsedAndConvertedCSV)
//         {
//             int identifier = pData[0];

//             if (identifier == (int)SaveDataIdentifier.Stats)
//             {
//                 PartyCharacter pc = new PartyCharacter(pData[1], pData[2], pData[3], pData[4], pData[5], pData[6]);
//                 GameContent.partyCharacters.AddLast(pc);
//             }
//             else if (identifier == (int)SaveDataIdentifier.Equipment)
//             {
//                 PartyCharacter lastMadePC = GameContent.partyCharacters.Last.Value;
//                 lastMadePC.equipment.AddLast(pData[1]);
//             }
//         }

//     }

