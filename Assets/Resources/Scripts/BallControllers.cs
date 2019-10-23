using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine;
public class AutoBallControl : BallControl
{
    
    private int[,] Maze = new int [MazeDescription.Rows, MazeDescription.Cols];
    private int[,] Coins = new int[2, MazeDescription.Coins];
            int[]  CoinsChange = new int[MazeDescription.Coins];
    private int NumCoin = 0, wayCount = 0;
    float xP, yP;
    int res = 0;
    int rsI = 0;
    bool reset = false;
    List<int> way = new List<int>();
    public override void SetMaze()
    {
        for (int q = 0; q< MazeDescription.Rows; q++)
        {
            for (int w = 0; w < MazeDescription.Cols; w++)
            {
                Maze[q,w] = -1;
            }
        }
        Maze[MazeDescription.Rows - 1, 0] = 0;
        MazeDescriptionVoid(0, 0);
        /*for (int q = 0; q < MazeDescription.Rows; q++)
        {
            string Log = "";
            for (int w = 0; w < MazeDescription.Cols; w++)
            {
                Log += Maze[q, w] + " ";
            }
            Debug.Log(Log);
        }*/
        CoinsSort();
        int max = Coins.Length / 2; for (int i = 0; i < max; i++) Debug.Log(Coins[0, i] + " : " + Coins[1, i] + " = " + Maze[MazeDescription.Rows - Coins[1, i] - 1, Coins[0, i]] + " = " + CoinsChange[i]);
        goToTheBall(Coins[1, 0], Coins[0, 0]);
    }
    public override int GetMove(float x, float y)
    {
        for (int i = 0; i < NumCoin; i++) DebugRay(Coins[0, i] * 4, Coins[1, i] * 4, 3f, Color.yellow);
        if (NumCoin != 0)
        {
            if (way.Count != 0)
            {
                if (!reset)
                {
                    res = way[wayCount - 1];
                    if (res == 1) yP++;
                    if (res == 2) xP++;
                    if (res == 4) yP--;
                    if (res == 8) xP--;
                    reset = true;
                }
            }
            else res = 0;
            if (res == 1) if (((float)(yP * 4) - 0.05f) < y) // вверх
                {
                    //Debug.Log("1: Y" + xP * 4 + "  -  " + x + "|||" + ((float)(yP * 4)-0.05f) + "  -  " + y);
                    reset = false;
                    way.RemoveAt(wayCount - 1);
                    wayCount--;                }
            if (res == 4) if (((float)(yP * 4) + 0.05f) > y) // вниз
                {
                    reset = false;
                    way.RemoveAt(wayCount - 1);
                    wayCount--;
                }
            if (res == 2) if (((float)(xP * 4) - 0.05f) < x) //вправо
                {
                    reset = false;
                    way.RemoveAt(wayCount - 1);
                    wayCount--;
                }
            if (res == 8) if (((float)(xP * 4) + 0.05f) > x) //влево
                {
                    reset = false;
                    way.RemoveAt(wayCount - 1);
                    wayCount--;
                }
            if (way.Count == 0)
            {
                Debug.LogError("Coin Succesfull");
                res = 0;
                rsI++;
                int ti = Coins[0, 0], ty = Coins[1, 0];
                resetMaze((int)x, (int)y);
                MazeDescriptionVoid((int)ti*4, (int)ty*4);
                if (rsI == 2)
                {
                    Debug.LogError("ONESTOPPED");
                    for (int q = 0; q < MazeDescription.Rows; q++)
                    {
                        string Log = "";
                        for (int w = 0; w < MazeDescription.Cols; w++)
                        {
                            Log += Maze[q, w] + "\t";
                        }
                        Debug.Log(Log);
                    }
                }
                CoinsSort();
                if (rsI != 2) goToTheBall(Coins[1, 0], Coins[0, 0]);
                reset = false;
            }
        }
        return res;
    }
    void resetMaze(int x1, int y1)
    {
        for (int q = 0; q < MazeDescription.Rows; q++)
        {
            for (int w = 0; w < MazeDescription.Cols; w++)
            {
                Maze[q, w] = -1;
            }
        }
        Maze[MazeDescription.Rows - 1 - Coins[1,0], 0 + Coins[0, 0]] = 0;
        /*Coins[1, 0] = -1;
        Coins[0, 0] = -1;*/
        NumCoin = 0;
        //for (int iw = 0; iw < wayCount; iw++) Debug.Log("WAY=" + way[iw] + "'");
        way.Clear();
        wayCount = 0;
        for (int iw = 0; iw < MazeDescription.Coins; iw++) Debug.Log(" " + Coins[0, iw] + " " + Coins[1, iw]);
        Debug.LogError("WAY.CLEANED");
    }
    public void MazeDescriptionVoid(int i, int y)
    {
        //if (rsI == 2)
        //{
        //    DebugRay(i, y, 4f, Color.red);
        //    //Debug.LogError("ONESTOPPED");
        //}
        int MazeI = i / 4, MazeY = y / 4;
        //Debug.Log("MazeDescriptionVoid(" + MazeI + " " + MazeY + ")");
        bool[] direction = RayCheck(i, y,1.45f, 3f);
        if (!direction[0]) // право
        {
            if (Maze[MazeDescription.Rows - 1 - MazeY, MazeI + 1] == -1)
            {
                Maze[MazeDescription.Rows - 1 - MazeY, MazeI + 1] = Maze[MazeDescription.Rows - 1 - MazeY, MazeI] + 1;
                //Debug.Log("Right| " + MazeY + " " + MazeI);
                MazeDescriptionVoid(i + 4, y);
            }
        }
        if (!direction[3]) // вверх
        {
            //Debug.Log("Up Non if| " + MazeI + " " + MazeY);
            if (Maze[MazeDescription.Rows - 2 - MazeY, MazeI] == -1)
            {
                //Debug.Log("Up| " + MazeY + " " + MazeI);
                Maze[MazeDescription.Rows - 2 - MazeY,MazeI] = Maze[MazeDescription.Rows - 1 - MazeY, MazeI] + 1;
                MazeDescriptionVoid(i, y + 4);
            }
        }
        if (!direction[2]) // лево
        {
            if (Maze[MazeDescription.Rows - 1 - MazeY, MazeI - 1] == -1)
            {
                //Debug.Log("Left| " + MazeY + " " + MazeI);
                Maze[MazeDescription.Rows - 1 - MazeY, MazeI - 1] = Maze[MazeDescription.Rows - 1 - MazeY,MazeI ] + 1;
                MazeDescriptionVoid(i - 4, y);
            }
        }
        if (!direction[1]) // вниз
        {
            if (Maze[MazeDescription.Rows - MazeY, MazeI ] == -1)
            {
                //Debug.Log("Bottom| " + MazeY + " " + MazeI);
                Maze[MazeDescription.Rows - MazeY, MazeI] = Maze[MazeDescription.Rows - 1 - MazeY, MazeI] + 1;
                MazeDescriptionVoid(i, y - 4);
            }
        }
        if (!direction[0]) RayStart(new Vector3(i, 1.45f, y), Vector3.right, 4f);
        if (!direction[1]) RayStart(new Vector3(i, 1.45f, y), Vector3.back, 4f);
        if (!direction[2]) RayStart(new Vector3(i, 1.45f, y), Vector3.left, 4f);
        if (!direction[3]) RayStart(new Vector3(i, 1.45f, y), Vector3.forward, 4f);
    }
    public bool[] RayCheck(float x, float y, float h, float Distance) // право, низ, лево, верх
    {
        //DebugRay(x, y, Distance, Color.red);
        bool[] Open = new bool[4];
        Open[0] = RayStart(new Vector3(x, h, y), Vector3.right, Distance);
        Open[1] = RayStart(new Vector3(x, h, y), Vector3.back, Distance);
        Open[2] = RayStart(new Vector3(x, h, y), Vector3.left, Distance);
        Open[3] = RayStart(new Vector3(x, h, y), Vector3.forward, Distance);
        return Open;
    }
    void DebugRay(float x, float y, float Distance, Color clr)
    {
        Vector3 start = new Vector3(x, 1.45f, y), forward = Vector3.right;
        //право
        Ray ray = new Ray(start, forward);
        Debug.DrawRay(start, ray.direction * Distance, clr);
        // низ
        forward = Vector3.back;
        ray = new Ray(start, forward);
        Debug.DrawRay(start, ray.direction * Distance, clr);
        // лево
        forward = Vector3.left;
        ray = new Ray(start, forward);
        Debug.DrawRay(start, ray.direction * Distance, clr);
        //верх
        forward = Vector3.forward;
        ray = new Ray(start, forward);
        Debug.DrawRay(start, ray.direction * Distance, clr);
    }
    void DebugRay(float x, float y, float Distance, Color clr, int v)
    {
        Vector3 start = new Vector3(x, 1.45f, y), forward = Vector3.right;
        if (v == 0) forward = Vector3.right;
        if (v == 1) forward = Vector3.back;
        if (v == 2) forward = Vector3.left;
        if (v == 3) forward = Vector3.forward;
        //право
        Ray ray = new Ray(start, forward);
        Debug.DrawRay(start, ray.direction * Distance, clr);
    }
    private bool RayStart(Vector3 start, Vector3 forward, float rayDistance)
    {
        Ray ray = new Ray(start, forward);
        RaycastHit hit;
        //Debug.DrawRay(start, ray.direction * rayDistance, Color.red);
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if (hit.collider.tag == "Coin")
            {
                Coins[0, NumCoin] = (int)(start.x + forward.x * 4) / 4;
                Coins[1, NumCoin] = (int)(start.z + forward.z * 4) / 4;
                CoinsChange[NumCoin] = Maze[MazeDescription.Rows - Coins[1, NumCoin] - 1, Coins[0, NumCoin]];
                NumCoin++;
                return false;
            }
            else
                if (hit.collider.name == "BallPrefab") return false;
                else return true;
        }
        else return false;
    }
    void CoinsSort()
    {
        int max = NumCoin;
        for (int i = 0; i < max; i++)
        {
           int minimum = CoinsChange[i],
               minimumN = i;
            for (int y = i + 1; y < max; y++)
            {
                if (CoinsChange[y] > 0)
                    if (CoinsChange[y] < minimum)
                    {
                        minimum = CoinsChange[y];
                        minimumN = y;
                    }
            }
            changeTwoPositionOnCoins(minimumN, i);
        }
    }
    void changeTwoPositionOnCoins(int ai, int bi)
    {
        if (ai != bi)
        {
            int pCoin0 = Coins[0, ai],
                pCoin1 = Coins[1, ai];
            Coins[0, ai] = Coins[0, bi];
            Coins[1, ai] = Coins[1, bi];
            Coins[0, bi] = pCoin0;
            Coins[1, bi] = pCoin1;
            int pCoinsChange = CoinsChange[ai];
            CoinsChange[ai] = CoinsChange[bi];
            CoinsChange[bi] = pCoinsChange;
        }
    }
    bool[] checkSide(int xM, int yM)
    {
        //Debug.Log(xM + " : " + yM
        bool[] Checed = new bool[4];
        for (int i = 0; i < Checed.Length; i++) Checed[i] = false;
        int thisPoint = Maze[MazeDescription.Rows - 1 - xM, yM];
        if (yM + 1 < MazeDescription.Cols)
        {
            if (Maze[MazeDescription.Rows - 1 - xM, yM + 1] == thisPoint - 1)
            { // справа
                Checed[0] = true;
                return Checed;
            }
        }
        if (MazeDescription.Rows - xM < MazeDescription.Rows)
        {
            if (Maze[MazeDescription.Rows - xM, yM] == thisPoint - 1)
            { // снизу
                Checed[1] = true;
                return Checed;
            }
        }
        if (yM - 1 >= 0)
        {
            if (Maze[MazeDescription.Rows - 1 - xM, yM - 1] == thisPoint - 1)
            { // слева
                Checed[2] = true;
                return Checed;
            }
        }
        if (MazeDescription.Rows - 2 - xM > 0)
        {
            if (Maze[MazeDescription.Rows - 2 - xM, yM] == thisPoint - 1)
            { // сверху
                Checed[3] = true;
                return Checed;
            }
        }
        return Checed;
    }
    void goToTheBall(int x, int y)
    {
        bool[] checkedSide;
        while (Maze[MazeDescription.Rows - 1 - (int)x, (int)y] != 0)
        {
            //DebugRay(y * 4, x * 4, 2f, Color.blue);
            if (rsI == 2)
            {
                //DebugRay(y * 4, x * 4, 2f, Color.blue);
                //Debug.LogError("ONESTOPPED");
            }
            bool[] direction = RayCheck(y*4, x*4, 1f, 3f);
            checkedSide = checkSide(x, y);
            if (checkedSide[0]) //справа
            {
                if (direction[0] == false)
                {
                    DebugRay(y * 4, x * 4, 4f, Color.blue, 0);
                    //Debug.Log("All Right");
                    way.Add(8);
                    y++;
                }
                else
                {
                    DebugRay(y * 4, x * 4, 3f, Color.red);
                    //Debug.Log("All  NOT Right");
                    Debug.LogError("STOP LEFT");
                }
            }
            if (checkedSide[1]) // снизу
            {
                if (direction[1] == false)
                {
                    DebugRay(y * 4, x * 4, 4f, Color.blue, 1);
                    way.Add(1);
                    x--;
                }
                else
                {
                    DebugRay(y * 4, x * 4, 3f, Color.blue);
                    //Debug.Log("All  NOT Right");
                    Debug.LogError("STOP UP");
                }
            }
            if (checkedSide[2]) // слева
            {
                if (direction[2] == false)
                {
                    DebugRay(y * 4, x * 4, 4f, Color.blue, 2);
                    way.Add(2);
                    y--;
                }
                else
                {
                    DebugRay(y * 4, x * 4, 3f, Color.black);
                    //Debug.Log("All  NOT Right");
                    Debug.LogError("STOP RIGHT");
                }
            }
            if (checkedSide[3]) // сверху
            {
                if (direction[3] == false)
                {
                    DebugRay(y * 4, x * 4, 4f, Color.blue, 3);
                    way.Add(4);
                    x++;
                }
                else
                {
                    DebugRay(y * 4, x * 4, 3f, Color.yellow);
                    //Debug.Log("All  NOT Right");
                    Debug.LogError("STOP DOWN");
                }
            }
        }
        String Log = "";
        for (int iw = 0; iw < way.Count; iw++) Log += way[iw];
        Debug.Log(Log);
        wayCount = way.Count;
        Debug.LogError("goToTheBall");
    }
}
