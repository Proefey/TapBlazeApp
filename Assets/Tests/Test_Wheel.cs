using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

//Tests only the method used to calculate the winner. Not to be confused with the play test
public class Test_Wheel
{
    private GameObject obj;
    private Wheel WS;

    [SetUp]
    public void Setup() {
        obj = new GameObject();
        WS = obj.AddComponent<Wheel>();
    }

    [Test]
    public void Test_Wheel_NoRigidBody() {
        WS.section_num = 3;
        WS.section_chances = new float[] { 30f, 30f, 40f };
        LogAssert.Expect(LogType.Error, $"No Rigidbody found on {obj.name}"); 
        WS.Start();
        
    }

    [Test]
    public void Test_Wheel_InvalidSectionNum() {
        WS.section_num = 0;
        WS.section_chances = new float[] { 30f, 30f, 40f };
        var rb = obj.AddComponent<Rigidbody2D>();
        LogAssert.Expect(LogType.Error, "Invalid Number of Sections: 0"); 
        WS.Start();
    }

    [Test]
    public void Test_Wheel_InvalidSectionChanceLength() {
        WS.section_num = 4;
        WS.section_chances = new float[] { 30f, 30f, 40f };
        var rb = obj.AddComponent<Rigidbody2D>();
        LogAssert.Expect(LogType.Error, "Incorrect length of section_chances. Expected: 4. Array Length: 3"); 
        WS.Start();
    }

    [Test]
    public void Test_Wheel_InvalidSectionChanceSum() {
        WS.section_num = 4;
        WS.section_chances = new float[] { 30f, 30f, 40f, 10f };
        var rb = obj.AddComponent<Rigidbody2D>();
        LogAssert.Expect(LogType.Error, "Chances do not add up to 100. Sum: 110"); 
        WS.Start();
    }

    [Test]
    public void Test_Wheel_EqualChances() {
        WS.section_num = 4;
        WS.section_chances = new float[] {};
        var rb = obj.AddComponent<Rigidbody2D>();
        WS.Start();
        int[] testresults = new int[] {0, 0, 0, 0};
        int testnum = 400;
        for(int i = 0; i < testnum; i++){
            int result = WS.win();
            testresults[result] += 1;
        }
        Debug.Log("Equal Chance Results:\n");
        Debug.Log("0: " + (float)testresults[0] / testnum * 100 + "\n1: " 
        + (float)testresults[1] / testnum * 100 + "\n2: " 
        + (float)testresults[2] / testnum * 100 + "\n3: " 
        + (float)testresults[3] / testnum * 100);
        int testsum = 0;
        foreach(int i in testresults){
            testsum += i;
        }
        Assert.AreEqual(testsum, testnum);
    }

    [Test]
    public void Test_Wheel_WheelTest() {
        WS.section_num = 8;
        WS.section_chances = new float[] {20f, 10f, 10f, 10f, 5f, 20f, 5f, 20f};
        var rb = obj.AddComponent<Rigidbody2D>();
        WS.Start();
        int[] testresults = new int[] {0, 0, 0, 0, 0, 0, 0, 0};
        int testnum = 1000;
        for(int i = 0; i < testnum; i++){
            int result = WS.win();
            testresults[result] += 1;
        }
        Debug.Log("Weighted Results:\n");
        Debug.Log(
        "Life 30 min: " + (float)testresults[0] / testnum * 100 + " (" + testresults[0] + " / " + testnum + " cases)" +
        "\nBrush 3x: " + (float)testresults[1] / testnum * 100 + " (" + testresults[1] + " / " + testnum + " cases)" +
        "\nGems 35: " + (float)testresults[2] / testnum * 100 + " (" + testresults[2] + " / " + testnum + " cases)" +
        "\nHammer 3x: " + (float)testresults[3] / testnum * 100 + " (" + testresults[3] + " / " + testnum + " cases)" +
        "\nCoins 750: " + (float)testresults[4] / testnum * 100 + " (" + testresults[4] + " / " + testnum + " cases)" +
        "\nBrush 1x: " + (float)testresults[5] / testnum * 100 + " (" + testresults[5] + " / " + testnum + " cases)" +
        "\nGems 75: " + (float)testresults[6] / testnum * 100 + " (" + testresults[6] + " / " + testnum + " cases)" +
        "\nHammer 1x: " + (float)testresults[7] / testnum * 100 + " (" + testresults[7] + " / " + testnum + " cases)");
        int testsum = 0;
        foreach(int i in testresults){
            testsum += i;
        }
        Assert.AreEqual(testsum, testnum);
    }

    [TearDown]
    public void Teardown() {
        Object.DestroyImmediate(obj);
    }
}
