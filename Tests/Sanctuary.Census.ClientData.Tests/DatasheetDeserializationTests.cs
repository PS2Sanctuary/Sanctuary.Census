using Microsoft.Win32.SafeHandles;
using Sanctuary.Census.ClientData.ClientDataModels;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Sanctuary.Census.ClientData.Tests;

public class DatasheetDeserializationTests
{
    [Fact]
    public void TestDeserialize()
    {
        using SafeFileHandle testDataFile = File.OpenHandle("Data\\ClientItemDatasheetData.txt");

        byte[] buffer = new byte[RandomAccess.GetLength(testDataFile)];
        RandomAccess.Read(testDataFile, buffer, 0);

        List<ClientItemDatasheetData> datas = ClientItemDatasheetData.Deserialize(buffer);
        Assert.Equal(30, datas.Count);

        ClientItemDatasheetData elementOne = new(1, 0, 1, 1, 500, 0, 0, 600, 0, 1, 673, 0);
        ClientItemDatasheetData element30 = new(84, 6, 28, 25, 725, 250, 0, 300, 3430, 1, 675, 3.5f);

        Assert.Equal(elementOne, datas[0]);
        Assert.Equal(element30, datas[29]);
    }
}
