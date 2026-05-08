using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer
{
    class Program
    {
        static void main( string[] args )
        {
            string ip = "127.0.0.1";
            int port = 7777;

            //방어코드  1 : 잘못된 포트 번호 들어올 시 유효성 검사 진행
            if ( port <= 0 || port > 65535 )
            {
                Console.WriteLine( "오류 : 유효하지 않은 Port 번호 입니다." );
                return;
            }
            try
            {
                TcpListener server = new TcpListener( IPAddress.Parse( ip ), port );
                server.Start();
                Console.WriteLine($"[서버] {ip}:{port} 에서 대기 중 ... ");

                while(true)
                {
                    using ( TcpClient client = server.AcceptTcpClient() )
                    //AcceptTcpClient => 누군가 접속 할때까지 일시 정지 (Blocking)
                    //using을 통해서 메모리 누수 방지
                    {
                        Console.WriteLine( "클라이언트가 접속했습니다.");
                        NetworkStream stream = client.GetStream();
                        //파이프 연결

                        byte [] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        //stream.Read → 파이프를 통해 들어온 데이터 담음.

                        //방어 코드 2 : 데이터가 비어있는지 확인
                        if ( bytesRead == 0 ) continue;

                        string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        //GetString  : human can read - 받음
                        Console.WriteLine($"수신 데이터: {received}" );

                        byte[] response = Encoding.UTF8.GetBytes($"[Echo] {received}");
                        //GetBytes : computer can read - 응답
                        stream.Write( response, 0, response.Length );
                    }
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"서버 오류 : {ex.Message}");
            } 
        }
    }
}