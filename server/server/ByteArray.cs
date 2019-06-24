using System;

public class ByteArray
{
    const int DEFAULT_SIZE = 2048;
    int initSize = 0;
    public byte[] bytes;
    public int readIdx = 0;
    public int writeIdx = 0;
    public int capacity = 0;
    public int remain { get { return capacity - writeIdx; } }
    public int Length { get { return writeIdx - readIdx; } }

    public ByteArray(int size = DEFAULT_SIZE) {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIdx = 0;
        writeIdx = 0;
    }

    public ByteArray(byte[] defaultBytes) {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initSize = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }

    public void CheckAndMoveBytes() {
        if (Length < 8) {
            MoveBytes();
        }
    }

    public void MoveBytes() {
        Array.Copy(bytes, readIdx, bytes, 0, Length);
        writeIdx = Length;
        readIdx = 0;
    }
}
