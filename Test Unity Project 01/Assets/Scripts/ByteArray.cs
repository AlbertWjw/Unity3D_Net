using System;

/// <summary>
/// 数据缓冲区类
/// </summary>
public class ByteArray
{
    const int DEFAULT_SIZE = 2048;  // 默认缓冲区大小

    public byte[] bytes;  // 缓冲区
    public int readIdx = 0;  // 读取位置
    public int writeIdx = 0;  // 写入位置
    public int capacity = 0;  // 缓冲区容量
    public int remain { get { return capacity - writeIdx; } }  // 剩余容量
    public int Length { get { return writeIdx - readIdx; } }  // 数据长度

    public ByteArray(int size = DEFAULT_SIZE) {
        bytes = new byte[size];
        capacity = size;
        readIdx = 0;
        writeIdx = 0;
    }

    public ByteArray(byte[] defaultBytes) {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }

    // 需要时，移动数据在缓冲区的位置
    public void CheckAndMoveBytes() {
        if (Length < 8) {
            MoveBytes();
        }
    }

    // 移动数据在缓冲区的位置
    public void MoveBytes() {
        Array.Copy(bytes, readIdx, bytes, 0, Length);
        writeIdx = Length;
        readIdx = 0;
    }
}
