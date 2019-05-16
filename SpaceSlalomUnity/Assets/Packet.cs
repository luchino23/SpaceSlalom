using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Packet : MonoBehaviour
{
    private MemoryStream stream;
    private BinaryWriter writer;

    private static uint packetCounter;

    private uint id;
    public uint Id
    {
        get
        {
            return id;
        }
    }

    public Packet()
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
        id = ++packetCounter;
    }

    public Packet(byte commandNumber, params object[] elements) : this()
    {
        writer.Write(commandNumber);
        foreach (object element in elements)
        {
            if (element is int)
            {
                writer.Write((int)element);
            }
            else if (element is float)
            {
                writer.Write((float)element);
            }
            else if (element is byte)
            {
                writer.Write((byte)element);
            }
            else if (element is char)
            {
                writer.Write((char)element);
            }
            else if (element is uint)
            {
                writer.Write((uint)element);
            }
            else
            {
                throw new Exception("unknown type");
            }
        }
    }

    public byte[] GetData()
    {
        return stream.ToArray();
    }
}
