using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PvpServer
{
    class Program
    {
        // 1. 접속한 모든 클라이언트를 관리하는 명단 (PVP를 위해 필수!)
        static List<TcpClient> clientList = new List<TcpClient>();

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 7777);
            server.Start();
            Console.WriteLine("PVP 서버 가동 중... (포트: 7777)");

            while (true)
            {
                // 새로운 플레이어가 올 때까지 대기
                TcpClient client = server.AcceptTcpClient();
                
                // 명단에 플레이어 추가 (여러 명의 일꾼이 동시에 건드리지 못하게 lock 사용)
                lock (clientList) { clientList.Add(client); }
                
                Console.WriteLine($"[입장] 새로운 플레이어가 들어왔습니다. (현재: {clientList.Count}명)");

                // 2. [핵심] 플레이어 전담 일꾼(Task) 배치
                // 이 줄 덕분에 한 명과 대화하는 동안에도 다른 사람의 접속을 받을 수 있습니다.
                Task.Run(() => HandleClient(client));
            }
        }

        static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    // 플레이어로부터 데이터 받기
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // 연결 종료 시 루프 탈출

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[수신]: {message}");

                    // 3. 브로드캐스트: 받은 정보를 모든 플레이어에게 전달
                    // 나중에 PVP에서 "플레이어 A가 총을 쐈다"는 정보를 모두에게 알릴 때 쓰입니다.
                    Broadcast(message);
                }
            }
            catch
            {
                // 연결 오류 시 처리
            }
            finally
            {
                // 플레이어 이탈 시 명단에서 삭제
                lock (clientList) { clientList.Remove(client); }
                client.Close();
                Console.WriteLine($"[퇴장] 플레이어 한 명이 나갔습니다. (현재: {clientList.Count}명)");
            }
        }

        static void Broadcast(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            
            // 명단에 있는 모든 수화기에 대고 똑같이 말해줍니다.
            lock (clientList)
            {
                foreach (var client in clientList)
                {
                    try { client.GetStream().Write(data, 0, data.Length); }
                    catch { /* 연결 끊긴 사람 무시 */ }
                }
            }
        }
    }
}