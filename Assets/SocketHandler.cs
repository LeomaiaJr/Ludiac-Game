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

	public TextMeshPro socketStatus;
	public TextMeshPro socketAnswer;
	private bool failedSocketConnection = false;
	private string serverMessage;

	private TcpClient socketConnection; 	
	private Thread clientReceiveThread; 	 		
	void Start () {
		ConnectToTcpServer();    
	}  	
	void Update () {         
		if (Input.GetKeyDown(KeyCode.Space)) {             
			SendMessage();         
		}

		if (socketConnection != null) {
			socketStatus.text = "Connected";
		}
		if (failedSocketConnection) {
			socketStatus.text = "Failed to connect";
		}
		if(serverMessage != null) {
			socketAnswer.text = serverMessage;
		}
		
	}  		
	private void ConnectToTcpServer () { 		
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();  		
		} 		
		catch (Exception e) { 			
			Debug.Log("On client connect exception " + e); 		
		} 	
	}  	 
	private void ListenForData() { 		
		try { 			
			socketConnection = new TcpClient("192.168.0.191", 123);
			Byte[] bytes = new Byte[1024];             
			while (true) { 							
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 									
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 								
						serverMessage = Encoding.ASCII.GetString(incommingData); 						
						Debug.Log("server message received as: " + serverMessage); 				
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {
			failedSocketConnection = true;            
			Debug.Log("Socket exception: " + socketException);         
		}     
	}  	
	private void SendMessage() {
        Debug.Log("Try to send a message");        
		if (socketConnection == null) {             
			return;         
		}  		
		try { 						
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				string clientMessage = "This is a message from one of your clients."; 				
               
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
              
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Client sent his message - should be received by server");             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}
}
