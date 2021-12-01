using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class SocketClient : MonoBehaviour
{

    public Image socketConnectedSignal;
    public RawImage sourceImage;


    string editString = "hello wolrd"; //編輯框文字

    Socket serverSocket; //伺服器端socket
    IPAddress ip; //主機ip
    IPEndPoint ipEnd;
    string recvStr; //接收的字串
    string sendStr; //傳送的字串
    int imageWidth = 512, imageHeight = 512;
    byte[] recvData = new byte[1000]; //接收的資料，必須為位元組
    byte[] sendData = new byte[1000]; //傳送的資料，必須為位元組
    int recvLen; //接收的資料長度
    Thread connectThread; //連線執行緒
   
    //初始化
    void InitSocket()
    {
        if (serverSocket != null)
        {
            if (serverSocket.Connected) return;
        }
        //定義伺服器的IP和埠，埠與伺服器對應
        ip = IPAddress.Parse("127.0.0.1"); //可以是區域網或網際網路ip，此處是本機
        ipEnd = new IPEndPoint(ip, 5566);


        //開啟一個執行緒連線，必須的，否則主執行緒卡死
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    void SocketConnet()
    {
        if (serverSocket != null)
            serverSocket.Close();
        //定義套接字型別,必須在子執行緒中定義
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            serverSocket.Connect(ipEnd);
        }
        catch (Exception e)
        {

            print(e);
        }
        //連線
       


     
       
    }

    public void paintFinish()
    {
        Texture2D _texture2D = TextureToTexture2D(sourceImage.texture);
        sendTexture2DToServer(_texture2D);
    }

     void sendTexture2DToServer(Texture2D _texture2D)
    {
        byte[] buf = Color32ArrayToByteArray(_texture2D.GetPixels32());//512 * 512 * 4

        int packageSize = 512;
        byte[] package = new byte[packageSize];

        for (int i = 0; i < buf.Length / packageSize; i++)
        {

            Array.Copy(buf, i * packageSize, package, 0, packageSize);
            serverSocket.Send(package, package.Length, SocketFlags.None);
        }
       
    }
    void SocketSend(string sendStr)
    {
        if (!serverSocket.Connected) return;
        //清空傳送快取
        sendData = new byte[1000];
        //資料型別轉換
        sendData = Encoding.ASCII.GetBytes(sendStr);
        //傳送
        serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    void SocketReceive()
    {
        SocketConnet();
        //不斷接收伺服器發來的資料
        while (serverSocket.Connected)
        {
            recvData = new byte[1000];
            recvLen = serverSocket.Receive(recvData);
            if (recvLen == 0)
            {
                SocketConnet();
                continue;
            }
            recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
            print(recvStr);
        }
    }

    void SocketQuit()
    {
        //關閉執行緒
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最後關閉伺服器
        if (serverSocket != null)
            serverSocket.Close();
        print("diconnect");
    }

    // Use this for initialization
    void Start()
    {
        InvokeRepeating("InitSocket", 1.0f,5.0f);
    }
    // Update is called once per frame
    void Update()
    {
        if (serverSocket !=null && socketConnectedSignal != null)
            socketConnectedSignal.color = serverSocket.Connected ? Color.green : Color.red;
    }

    //程式退出則關閉連線
    void OnApplicationQuit()
    {
        SocketQuit();
    }

    private static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = lengthOfColor32 * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        return bytes;
    }
  
    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height),0,0);
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;
    }
}
