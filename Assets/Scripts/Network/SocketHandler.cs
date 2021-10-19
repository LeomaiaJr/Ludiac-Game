using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class SocketHandler : MonoBehaviour
{

    public TextMesh socketStatus;
    private bool failedSocketConnection = false;
    private string serverMessage;

    private bool prevLeftSensorRead = false;
    private bool prevRightSensorRead = false;
    private bool levelTwo = false;
    private float firstAnswer = 3.8f;
    private float secondAnswer = 4.6f;

    private bool alternateOption = false;
    private bool selectOption = false;

    public GameObject selectGameObject;

    private TcpClient socketConnection;
    private Thread clientReceiveThread;

    public GameObject spongeBobGameObject;
    public GameObject patrickGameObject;
	public TextMesh guessWord;
	public TextMesh firstOption;
	public TextMesh secondOption;
	public TextMesh thirdOption;
	public TextMesh fourthOption;

    void Start()
    {
        ConnectToTcpServer();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessage();
        }

        if (socketConnection != null)
        {
            socketStatus.text = "Controle: Conectado";
            socketStatus.color = Color.green;
        }
        if (failedSocketConnection)
        {
            socketStatus.text = "Controle: Desconectado";
            socketStatus.color = Color.red;
        }
        if (alternateOption)
        {
            if (selectGameObject.transform.position.y < 3.9f && selectGameObject.transform.position.y > 3.7f)
            {
                selectGameObject.transform.position = new Vector3(selectGameObject.transform.position.x, 6.2f, selectGameObject.transform.position.z);
            }
            else
            {
                selectGameObject.transform.position = new Vector3(selectGameObject.transform.position.x, selectGameObject.transform.position.y - 0.8f, selectGameObject.transform.position.z);
            }
            alternateOption = false;
        }
        if (selectOption)
        {
            CheckSelectedOption();
            selectOption = false;
        }
    }
    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient("192.168.43.91", 123);
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        serverMessage = Encoding.ASCII.GetString(incommingData);
                        try
                        {
                            string[] data = serverMessage.Split(';');
                            bool leftSensorRead = data[0].Substring(4, 1) == "0" ? true : false;
                            bool rightSensorRead = data[1].Substring(5, 1) == "0" ? true : false;

                            Debug.Log("Left: " + leftSensorRead + " Right: " + rightSensorRead);

                            if (rightSensorRead != prevRightSensorRead)
                            {
                                alternateOption = rightSensorRead;
                            }
                            if (leftSensorRead != prevLeftSensorRead)
                            {
                                selectOption = leftSensorRead;
                            }
                            prevRightSensorRead = rightSensorRead;
                            prevLeftSensorRead = leftSensorRead;
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Exception when reading the sensors values " + e);
                        }
                        //Debug.Log("server message received as: " + serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            failedSocketConnection = true;
            Debug.Log("Socket exception: " + socketException);
        }
    }
    private void SendMessage()
    {
        Debug.Log("Try to send a message");
        if (socketConnection == null)
        {
            return;
        }
        try
        {
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = "This is a message from one of your clients.";

                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);

                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    void CheckSelectedOption()
    {
        if (!levelTwo)
        {
            if (selectGameObject.transform.position.y < (firstAnswer + 0.1f) && selectGameObject.transform.position.y > (firstAnswer - 0.1f))
            {
                Debug.Log("Correct");
                spongeBobGameObject.SetActive(false);
				patrickGameObject.SetActive(true);
				guessWord.text = "ESTRE.......";
				firstOption.text = "LE";
				secondOption.text = "LO";
				thirdOption.text = "LA";
				fourthOption.text = "LI";
            }
            else
            {
                Debug.Log("Wrong");
            }
        }
    }
}

