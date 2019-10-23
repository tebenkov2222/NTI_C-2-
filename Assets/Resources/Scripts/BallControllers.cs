using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine;
public class AutoBallControl : BallControl
{
    
    private int[,] Maze = new int [MazeDescription.Rows, MazeDescription.Cols]; // массив расстояния от мячика на данный момент до каждой следующей ячейки [yMap,xMap]
    private int[,] Coins = new int[2, MazeDescription.Coins]; // массив всех монет (стоило бы вектором. но сложнее) [xMap or yMap, coins]
            int[]  CoinsChange = new int[MazeDescription.Coins]; // массив расстояния до каждой из монет [xMap or yMap, coins]
    private int NumCoin = 0, wayCount = 0; // первое служит для переключения в цикле монет, второе для переключения пути в цикле.
    List<int> way = new List<int>(); // путь, по которому должен двигаться шарик до ближайшей монетки
    float xP, yP; //x past, y past
    int wayThis = 0; // значение way на данный момент шага шарика 
    int coinsThis = 0; // дебаговая переменная для проверки 3 монетки
    bool reset = false; // булева для ожидания и обновления следующего хода way
    public override void SetMaze() //void Start() условно
    {
        // обнуляем  весь массив
        for (int q = 0; q< MazeDescription.Rows; q++)
        {
            for (int w = 0; w < MazeDescription.Cols; w++)
            {
                Maze[q,w] = -1;
            }
        }
        // не забываем, что шарик находтся в точке 0,0 изначально ( y = 0 = MazeDescription.Rows - 1, Идет обратный порядок)
        Maze[MazeDescription.Rows - 1, 0] = 0;
        //Выполняем анализ всей карты от точки 0,0
        MazeDescriptionVoid(0, 0);
        /// DEBUG всей карты
        /*for (int q = 0; q < MazeDescription.Rows; q++)
        {
            string Log = "";
            for (int w = 0; w < MazeDescription.Cols; w++)
            {
                Log += Maze[q, w] + " ";
            }
            Debug.Log(Log);
        }*/
        // сортировка всех монет по порядку
        CoinsSort();
        ///DEBUG
        int max = Coins.Length / 2; for (int i = 0; i < max; i++) Debug.Log(Coins[0, i] + " : " + Coins[1, i] + " = " + Maze[MazeDescription.Rows - Coins[1, i] - 1, Coins[0, i]] + " = " + CoinsChange[i]);
        //находим путь от точки 0,0 до ближайшей монетки
        goToTheBall(Coins[0, 0], Coins[1, 0]);
    }
    public override int GetMove(float x, float y) //void FixedUpdate() условно Возвращает данное положения шарика на карте
    {
        /// DEBUG Coins на карте, для определения всех монеток, чтобы было видно на карте
        for (int i = 0; i < NumCoin; i++) DebugRay(Coins[0, i] * 4, Coins[1, i] * 4, 3f, Color.yellow);
        //если колличество монеток не равно 0
        if (NumCoin != 0)
        {
            // то если и путь до ближайшей монетки не равен пустой строке
            if (way.Count != 0)
            {
                // то если можно двигаться по карте дальше
                if (!reset)
                {
                    // находим куда именно надо двигаться
                    wayThis = way[wayCount - 1];
                    if (wayThis == 1) yP++;
                    if (wayThis == 2) xP++;
                    if (wayThis == 4) yP--;
                    if (wayThis == 8) xP--;
                    reset = true; // булеву закроем до следующего шага
                }
            }
            else wayThis = 0; // а если длина пути равно нулю (то есть дошел шарик до монетки. то мы никуда не двигаемся)
            // дальше идет код, который определяет, перешел ли шарик на другой шаг (то есть перешел ли шарик на одну еденицу пути в нужную сторону
            if (wayThis == 1) if (((float)(yP * 4) - 0.05f) < y) // вверх
                {
                    //Debug.Log("1: Y" + xP * 4 + "  -  " + x + "|||" + ((float)(yP * 4)-0.05f) + "  -  " + y);
                    reset = false;
                    way.RemoveAt(wayCount - 1);
                    wayCount--;                }
            if (wayThis == 4) if (((float)(yP * 4) + 0.05f) > y) // вниз
                {
                    reset = false;
                    way.RemoveAt(wayCount - 1);
                    wayCount--;
                }
            if (wayThis == 2) if (((float)(xP * 4) - 0.05f) < x) //вправо
                {
                    reset = false;
                    way.RemoveAt(wayCount - 1);
                    wayCount--;
                }
            if (wayThis == 8) if (((float)(xP * 4) + 0.05f) > x) //влево
                {
                    reset = false;
                    way.RemoveAt(wayCount - 1);
                    wayCount--;
                }
            // если все так и путь пустой (в идеале дошло до монетки)
            if (way.Count == 0)
            {
                Debug.LogError("Coin Succesfull"); // продебажить надо
                wayThis = 0; // останавливаем шарик
                coinsThis++; // переходим на следующую монетку
                int ty = Coins[0, 0], ti = Coins[1, 0]; // сохраняем данные прошлой монетки. Эти даннные будут месторасположением монетки на карте
                resetMaze((int)x, (int)y); // обнуляем карту
                MazeDescriptionVoid((int)ti*4, (int)ty*4); // выполняем анализ карты уже от данных прошлой монетки (м-я мячика)
                if (coinsThis == 2) // вот дебаг 3 монетки
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
                CoinsSort(); // сортировка монет, все по стандарту
                if (coinsThis != 2) goToTheBall(Coins[0, 0], Coins[1, 0]); // если мы не на 3 монетке, то пытаемся найти путь
                reset = false; //  даем понять, что надо куда то двигаться
            }
        }
        return wayThis; // возвращаем направление движения шарика 
    }
    void resetMaze(int x1, int y1) // обнуление карты
    {
        // обнуляем масу
        for (int q = 0; q < MazeDescription.Rows; q++)
        {
            for (int w = 0; w < MazeDescription.Cols; w++)
            {
                Maze[q, w] = -1;
            }
        }
        Maze[MazeDescription.Rows - 1 - Coins[0,0], 0 + Coins[1, 0]] = 0; // данные шарика за 0
        NumCoin = 0; // колличество монеток обнуляем
        //for (int iw = 0; iw < wayCount; iw++) Debug.Log("WAY=" + way[iw] + "'");
        way.Clear(); // дополнительно чистим путь 
        wayCount = 0; // каунт тоже
        /// банальный DEBUG
        for (int iw = 0; iw < MazeDescription.Coins; iw++) Debug.Log(" " + Coins[1, iw] + " " + Coins[0, iw]);
        Debug.LogError("WAY.CLEANED");
    }
    public void MazeDescriptionVoid(int xMap, int yMap) // дошли до самого интересного АНАЛИЗ КАРТЫ. 
   /* xMap - приемка z (x в плоскости) положения шарика на карте
    * yMap - приемка x (y в плоскости) положения шарика на карте
    * рекурсиная функция
    * */
    {

        /// снова DEBUG
        //if (coinsThis == 2)
        //{
        //    DebugRay(xMap, yMap, 4f, Color.red);
        //    //Debug.LogError("ONESTOPPED");
        //}
        int xMaze = xMap / 4, yMaze = yMap / 4; 
        ///DEBUG
        //Debug.Log("MazeDescriptionVoid(" + xMaze + " " + yMaze + ")");
        bool[] direction = RayCheck(xMap, yMap,1.45f, 3f); //проверяем ввсе 4 направления на наличие стенки
        /* возвращает тру если есть стенка. иначе ф [право, низ, лево, верх]
         * 
         * если мы не были еще в этой точке то 
         * проверяем на монетку данную ячейку
         * присваем значение данной ячейки предыдущей + 1
         * и вызываем рекурсию от этой точки
         * */
        if (!direction[0]) // право
        {
            if (Maze[MazeDescription.Rows - 1 - yMaze, xMaze + 1] == -1) // 
            {
                RayStart(new Vector3(xMap, 1.45f, yMap), Vector3.right, 4f);
                Maze[MazeDescription.Rows - 1 - yMaze, xMaze + 1] = Maze[MazeDescription.Rows - 1 - yMaze, xMaze] + 1;
                //Debug.Log("Right| " + yMaze + " " + xMaze);
                MazeDescriptionVoid(xMap + 4, yMap);
            }
        }
        if (!direction[3]) // вверх
        {
            //Debug.Log("Up Non if| " + xMaze + " " + yMaze);
            if (Maze[MazeDescription.Rows - 2 - yMaze, xMaze] == -1)
            {
                RayStart(new Vector3(xMap, 1.45f, yMap), Vector3.forward, 4f);
                //Debug.Log("Up| " + yMaze + " " + xMaze);
                Maze[MazeDescription.Rows - 2 - yMaze,xMaze] = Maze[MazeDescription.Rows - 1 - yMaze, xMaze] + 1;
                MazeDescriptionVoid(xMap, yMap + 4);
            }
        }
        if (!direction[2]) // лево
        {
            if (Maze[MazeDescription.Rows - 1 - yMaze, xMaze - 1] == -1)
            {
                RayStart(new Vector3(xMap, 1.45f, yMap), Vector3.left, 4f);
                //Debug.Log("Left| " + yMaze + " " + xMaze);
                Maze[MazeDescription.Rows - 1 - yMaze, xMaze - 1] = Maze[MazeDescription.Rows - 1 - yMaze,xMaze ] + 1;
                MazeDescriptionVoid(xMap - 4, yMap);
            }
        }
        if (!direction[1]) // вниз
        {
            if (Maze[MazeDescription.Rows - yMaze, xMaze ] == -1)
            {
                RayStart(new Vector3(xMap, 1.45f, yMap), Vector3.back, 4f);
                //Debug.Log("Bottom| " + yMaze + " " + xMaze);
                Maze[MazeDescription.Rows - yMaze, xMaze] = Maze[MazeDescription.Rows - 1 - yMaze, xMaze] + 1;
                MazeDescriptionVoid(xMap, yMap - 4);
            }
        }
    }
    public bool[] RayCheck(float xMap, float yMap, float heigh_on_Floor, float Distance) //   функция проверки всех 4 сторон на наличие стенок и наличие монеток в следующей точке
    /* принимает точка, от которых будет идти проверка, так же высоту, на которой будет пускаться луч и дистанцию соответсвенно
     * возвращает булевы [право, низ, лево, верх]
    * */
    {
        ///DEBUG
        //DebugRay(xMap, yMap, Distance, Color.red);
        bool[] Open = new bool[4];
        Open[0] = RayStart(new Vector3(xMap, heigh_on_Floor, yMap), Vector3.right, Distance);
        Open[1] = RayStart(new Vector3(xMap, heigh_on_Floor, yMap), Vector3.back, Distance);
        Open[2] = RayStart(new Vector3(xMap, heigh_on_Floor, yMap), Vector3.left, Distance);
        Open[3] = RayStart(new Vector3(xMap, heigh_on_Floor, yMap), Vector3.forward, Distance);
        return Open;
    }
    void DebugRay(float xMap, float yMap, float Distance, Color clr) /// тупо DEBUG
    /* просто дебаг данной точки при данных x и  y на карте и цвете
     * */
    {
        Vector3 start = new Vector3(xMap, 1.45f, yMap), forward = Vector3.right;
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
    void DebugRay(float xMap, float yMap, float Distance, Color clr, int direction) ///DEBUG
    /* тоже дебаг только в определенном направлении. данные дебага те же с добавкой направления [право, низ, лево, верх]
     * */
    {
        Vector3 start = new Vector3(xMap, 1.45f, yMap), forward = Vector3.right;
        if (direction == 0) forward = Vector3.right;
        if (direction == 1) forward = Vector3.back;
        if (direction == 2) forward = Vector3.left;
        if (direction == 3) forward = Vector3.forward;
        //право
        Ray ray = new Ray(start, forward);
        Debug.DrawRay(start, ray.direction * Distance, clr);
    }
    private bool RayStart(Vector3 start, Vector3 forward, float rayDistance) // пускаем луч
    /* приемка: начальный вектор, вектор, в сторону которого направлен луч, и дистанция луча
     * возврат: если нет стены, то ф 
     *          иначе т
     * если найдена в данной точке монетка то  добавляем в массив монетку, вычисляем ее значение и увеличивам счетчик монет
     * Coin[yMsp,xMap]
     * */
    
    {
        Ray ray = new Ray(start, forward);
        RaycastHit hit;
        //Debug.DrawRay(start, ray.direction * rayDistance, Color.red);
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if (hit.collider.tag == "Coin")
            {
                Coins[0, NumCoin] = (int)(start.z + forward.z * 4) / 4;
                Coins[1, NumCoin] = (int)(start.x + forward.x * 4) / 4;
                CoinsChange[NumCoin] = Maze[MazeDescription.Rows - Coins[0, NumCoin] - 1, Coins[1, NumCoin]];
                NumCoin++;
                return false;
            }
            else
                if (hit.collider.name == "BallPrefab") return false;
                else return true;
        }
        else return false;
    }
    void CoinsSort() // сортировка монеток
    /* обычная сортировка монет по расстоянию от шарика на данный момент времени
     * 
     * */
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
    void changeTwoPositionOnCoins(int ai, int bi) // меняет позиции монеток в массиве согласно проавилам обмена
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
    bool[] checkSide(int yMaze, int xMaze) // проверка на наличие точки минимального расстояния
    /* приемка: данные проверки в мейзе
     * возвратка: булевы [право, низ, лево, верх]
     * присваиваем значение в массиве данной точки
     * если по направлени не выходит за границы лабиринта то 
     *      если расстояния следующей точки меньше расстоянию этой точки на 1 то
     *          итератору возвращаем т
     * */
    {
        //Debug.Log(yMaze + " : " + xMaze
        bool[] return_bool = new bool[4];
        for (int i = 0; i < return_bool.Length; i++) return_bool[i] = false;
        int thisPoint = Maze[MazeDescription.Rows - 1 - yMaze, xMaze];
        if (xMaze + 1 < MazeDescription.Cols)
            if (Maze[MazeDescription.Rows - 1 - yMaze, xMaze + 1] == thisPoint - 1) // справа
                return_bool[0] = true;
        if (MazeDescription.Rows - yMaze < MazeDescription.Rows)
            if (Maze[MazeDescription.Rows - yMaze, xMaze] == thisPoint - 1) // снизу
                return_bool[1] = true;
        if (xMaze - 1 >= 0)
            if (Maze[MazeDescription.Rows - 1 - yMaze, xMaze - 1] == thisPoint - 1) // слева
                return_bool[2] = true;
        if (MazeDescription.Rows - 2 - yMaze > 0)
            if (Maze[MazeDescription.Rows - 2 - yMaze, xMaze] == thisPoint - 1) // сверху
                return_bool[3] = true;
        return return_bool;
    }
    void goToTheBall(int yMaze, int xMaze) // нахождение минимального пути до монеты
    /* приемка: значения точки на карте 
     * возвратка: way
     * пока данная точка не равна 0
     *  1 = находим стороны, в которых расстояния до мячика меньше ровно на единицу
     *  2 = находим стороны, в которые можно пойти (не преграждает стенка)
     *  если расстояние от мячика меньше на единицу и стенки нет, то добавляем в конец противоположное направление
     * */
    {
        bool[] checkedSide;
        while (Maze[MazeDescription.Rows - 1 - yMaze, xMaze] != 0)
        {
            ///DEBUG
            //DebugRay(xMaze * 4, yMaze * 4, 2f, Color.blue);
            if (coinsThis == 2)
            {
                //DebugRay(xMaze * 4, yMaze * 4, 2f, Color.blue);
                //Debug.LogError("ONESTOPPED");
            }

            bool[] direction = RayCheck(xMaze*4, yMaze*4, 1f, 3f);
            checkedSide = checkSide(yMaze, xMaze);
            if (checkedSide[0]) //справа
            {
                if (direction[0] == false)
                {
                    DebugRay(xMaze * 4, yMaze * 4, 4f, Color.blue, 0);
                    //Debug.Log("All Right");
                    way.Add(8);
                    xMaze++;
                }
                else
                {
                    DebugRay(xMaze * 4, yMaze * 4, 3f, Color.red);
                    //Debug.Log("All  NOT Right");
                    Debug.LogError("STOP LEFT");
                }
            }
            if (checkedSide[1]) // снизу
            {
                if (direction[1] == false)
                {
                    DebugRay(xMaze * 4, yMaze * 4, 4f, Color.blue, 1);
                    way.Add(1);
                    yMaze--;
                }
                else
                {
                    DebugRay(xMaze * 4, yMaze * 4, 3f, Color.blue);
                    //Debug.Log("All  NOT Right");
                    Debug.LogError("STOP UP");
                }
            }
            if (checkedSide[2]) // слева
            {
                if (direction[2] == false)
                {
                    DebugRay(xMaze * 4, yMaze * 4, 4f, Color.blue, 2);
                    way.Add(2);
                    xMaze--;
                }
                else
                {
                    DebugRay(xMaze * 4, yMaze * 4, 3f, Color.black);
                    //Debug.Log("All  NOT Right");
                    Debug.LogError("STOP RIGHT");
                }
            }
            if (checkedSide[3]) // сверху
            {
                if (direction[3] == false)
                {
                    DebugRay(xMaze * 4, yMaze * 4, 4f, Color.blue, 3);
                    way.Add(4);
                    yMaze++;
                }
                else
                {
                    DebugRay(xMaze * 4, yMaze * 4, 3f, Color.yellow);
                    //Debug.Log("All  NOT Right");
                    Debug.LogError("STOP DOWN");
                }
            }
        }
        ///DEBUG
        String Log = "";
        for (int iw = 0; iw < way.Count; iw++) Log += way[iw];
        Debug.Log(Log);
        wayCount = way.Count;
        Debug.LogError("goToTheBall");
    }
}
