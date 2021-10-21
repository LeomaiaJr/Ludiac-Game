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

    public AudioSource gameIntro;
    public AudioSource bobIntro;
    public AudioSource bobQuestion;
    public AudioSource bobSuccess;
    public AudioSource bobFailed;
    public AudioSource patrickIntro;
    public AudioSource patrickQuestion;
    public AudioSource patrickSuccess;
    public AudioSource patrickFailed;

    public AudioSource changeOption;
    public AudioSource wrongOption;
    public AudioSource correctOption;
    private bool canChangeAlternative = false;

    public GameObject alternatives;
    public GameObject correct;

    private float firstTime = 5f;
    private bool turnOffFirstTime = false;
    private float gameIntroTime = 10f;
    private bool turnOffIntroTime = true;

    private float bobIntroTime = 15f;
    private bool turnOffBobIntroTime = true;

    private float bobQuestionTime = 7f;
    private bool turnOffBobQuestionTime = true;

    private float nextQuestionTime = 5f;
    private bool turnOffNextQuestionTime = true;

    private float patrickIntroTime = 23f;
    private bool turnOffPatrickIntroTime = true;

    private float patrickQuestionTime = 8f;
    private bool turnOffPatrickQuestionTime = true;

    private float restartTime = 6f;
    private bool turnOffRestartTime = true;

    void Start()
    {
        ConnectToTcpServer();
    }
    void Update()
    {

        if (!turnOffFirstTime)
        {
            firstTime -= Time.deltaTime;
        }
        if (firstTime < 0 && !turnOffFirstTime)
        {
            turnOffFirstTime = true;
            turnOffIntroTime = false;
            gameIntro.Play();
        }
        if (!turnOffIntroTime)
        {
            gameIntroTime -= Time.deltaTime;
        }
        if (gameIntroTime < 0 && !turnOffIntroTime)
        {
            bobIntro.Play();
            turnOffIntroTime = true;
            turnOffBobIntroTime = false;
        }
        if (!turnOffBobIntroTime)
        {
            bobIntroTime -= Time.deltaTime;
        }
        if (bobIntroTime < 0 && !turnOffBobIntroTime)
        {
            turnOffBobIntroTime = true;
            turnOffBobQuestionTime = false;
            bobQuestion.Play();
        }
        if (!turnOffBobQuestionTime)
        {
            bobQuestionTime -= Time.deltaTime;
        }
        if (bobQuestionTime < 0 && !turnOffBobQuestionTime)
        {
            canChangeAlternative = true;
            turnOffBobQuestionTime = true;
        }
        if (!turnOffNextQuestionTime)
        {
            nextQuestionTime -= Time.deltaTime;
        }
        if (nextQuestionTime < 0 && !turnOffNextQuestionTime)
        {
            turnOffNextQuestionTime = true;
            patrickGameObject.SetActive(true);
            alternatives.SetActive(true);
            guessWord.text = "ESTRE.......";
            correct.SetActive(false);
            selectGameObject.SetActive(true);
            turnOffPatrickIntroTime = false;
            patrickIntro.Play();
        }
        if (!turnOffPatrickIntroTime)
        {
            patrickIntroTime -= Time.deltaTime;
        }
        if (patrickIntroTime < 0 && !turnOffPatrickIntroTime)
        {
            patrickQuestion.Play();
            turnOffPatrickIntroTime = true;
            turnOffPatrickQuestionTime = false;
        }
        if (!turnOffPatrickQuestionTime)
        {
            patrickQuestionTime -= Time.deltaTime;
        }
        if (patrickQuestionTime < 0 && !turnOffPatrickQuestionTime)
        {
            turnOffPatrickQuestionTime = true;
            canChangeAlternative = true;
        }
        if (!turnOffRestartTime)
        {
            restartTime -= Time.deltaTime;
        }
        if (restartTime < 0 && !turnOffRestartTime)
        {
            turnOffRestartTime = true;
            RestartGame();
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessage();
        }

        if (socketConnection != null)
        {
            socketStatus.text = "Controle: Conectado";
            socketStatus.color = Color.green;
            //CreateTimer("gameIntro", 0.5f, 0, false, teste);
        }
        if (failedSocketConnection)
        {
            socketStatus.text = "Controle: Desconectado";
            socketStatus.color = Color.red;
        }
        if (alternateOption && canChangeAlternative)
        {
            if (selectGameObject.transform.position.y < 3.9f && selectGameObject.transform.position.y > 3.7f)
            {
                selectGameObject.transform.position = new Vector3(selectGameObject.transform.position.x, 6.2f, selectGameObject.transform.position.z);
            }
            else
            {
                selectGameObject.transform.position = new Vector3(selectGameObject.transform.position.x, selectGameObject.transform.position.y - 0.8f, selectGameObject.transform.position.z);
            }
            changeOption.Play();
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
            socketConnection = new TcpClient("192.168.0.191", 123);
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
        if (canChangeAlternative)
        {
            if (!levelTwo)
            {
                if (selectGameObject.transform.position.y < (firstAnswer + 0.1f) && selectGameObject.transform.position.y > (firstAnswer - 0.1f))
                {
                    bobSuccess.Play();
                    Debug.Log("Correct");
                    correctOption.Play();
                    turnOffNextQuestionTime = false;
                    spongeBobGameObject.SetActive(false);

                    levelTwo = true;
                    canChangeAlternative = false;

                    selectGameObject.SetActive(false);
                    alternatives.SetActive(false);
                    patrickGameObject.SetActive(false);
                    guessWord.text = "";
                    correct.SetActive(true);
                    firstOption.text = "LE";
                    secondOption.text = "LO";
                    thirdOption.text = "LA";
                    fourthOption.text = "LI";
                }
                else
                {
                    wrongOption.Play();
                    bobFailed.Play();
                    Debug.Log("Wrong");
                }
            }
            else
            {
                if (selectGameObject.transform.position.y < (secondAnswer + 0.1f) && selectGameObject.transform.position.y > (secondAnswer - 0.1f))
                {
                    patrickSuccess.Play();
                    Debug.Log("Correct Patrick");
                    correctOption.Play();

                    canChangeAlternative = false;
                    alternatives.SetActive(false);
                    selectGameObject.SetActive(false);
                    patrickGameObject.SetActive(false);
                    guessWord.text = "";
                    correct.SetActive(true);
                    turnOffRestartTime = false;
                }
                else
                {
                    wrongOption.Play();
                    patrickFailed.Play();
                    Debug.Log("Wrong Patrick");
                }
            }
        }
    }

    void RestartGame()
    {
        levelTwo = false;
        canChangeAlternative = false;

        bobIntroTime = 15f;
        turnOffBobIntroTime = true;

        bobQuestionTime = 7f;
        turnOffBobQuestionTime = true;

        nextQuestionTime = 5f;
        turnOffNextQuestionTime = true;

        patrickIntroTime = 23f;
        turnOffPatrickIntroTime = true;

        patrickQuestionTime = 8f;
        turnOffPatrickQuestionTime = true;

        restartTime = 6f;
        turnOffRestartTime = true;
        alternatives.SetActive(true);
        selectGameObject.SetActive(true);
        patrickGameObject.SetActive(false);
        spongeBobGameObject.SetActive(true);
        correct.SetActive(false);
        guessWord.text = "ES.......JA";
        firstOption.text = "PAN";
        secondOption.text = "PEN";
        thirdOption.text = "PIN";
        fourthOption.text = "PON";

        bobIntro.Play();
        turnOffBobIntroTime = false;
    }
}
