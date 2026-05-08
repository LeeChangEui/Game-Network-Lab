using System;
using System.Net.Sockets;
using System.Text;

namespace EchoClient
{
    class Program
    {
        static void main( string[] args )
        {
            try
            {
                TcpClient client = new TcpClient( "127.0.0.1", 7777 );
                //server.AccpetTcpClient() 실행
                Console.WriteLine( "서버에 접속했습니다." );
                
                string message = "Hello, Network! ";
                byte[] data = Encoding.UTF8.GetBytes( message );
                //기계어로 번역
                //ex) hello -> [72, 101, 108, 108, 111] 

                NetworkStream stream = client.GetStream();
                //서버 연결 성공 후 파이프 ㅅ생성 

                stream.Write( data, 0, data.Length ); //파이프에 데이터 밀어넣기

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read( buffer, 0, buffer.Length ); //서버가 보내는 답장 기다림
                Console.WriteLine( $"서버 응답: {Encoding.UTF8.GetString(buffer, 0, bytesRead)}" );

                client.Close();
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"접속 실패 : {ex.Message} ");
            }
        }
    }
}