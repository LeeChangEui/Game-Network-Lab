using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PvpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // 1. 서버 접속 (내 컴퓨터의 7777 포트)
                TcpClient client = new TcpClient("127.0.0.1", 7777);
                Console.WriteLine("서버에 접속했습니다!");
                Console.WriteLine("메시지를 입력하면 모든 플레이어에게 전달됩니다. (종료: Ctrl+C)");

                NetworkStream stream = client.GetStream();

                // 2. [핵심] 서버가 보내는 정보를 실시간으로 듣는 일꾼(Task) 생성
                // 이 줄 덕분에 내가 채팅을 입력하는 중에도 남이 보낸 메시지가 화면에 뜹니다.
                Task.Run(() => ReceiveMessages(stream));

                // 3. 메인 루프: 사용자의 입력을 서버로 전송
                while (true)
                {
                    string message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message)) continue;

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"접속 오류: {ex.Message}");
            }
        }

        // 서버로부터 데이터를 무한정 대기하고 받는 전용 함수
        static void ReceiveMessages(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    // 서버가 보낼 때까지 여기서 대기 (Blocking)
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    
                    // 서버가 연결을 끊으면 0이 리턴됨
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    // 화면을 살짝 예쁘게 출력
                    Console.WriteLine($"\n[상대방]: {message}");
                    Console.Write("> "); // 입력 프롬프트 유지
                }
            }
            catch
            {
                Console.WriteLine("\n서버와의 연결이 끊겼습니다.");
            }
        }
    }
}