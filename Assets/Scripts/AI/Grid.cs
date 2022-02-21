﻿using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Grid : MonoBehaviour
{
    //  Модель для отрисовки узла сетки
    public GameObject nodeModel;

    bool check = true;

    //  Ландшафт (Terrain) на котором строится путь
    [SerializeField] private Terrain landscape = null;

    //  Шаг сетки (по x и z) для построения точек
    [SerializeField] private int gridDelta = 20;

    //  Номер кадра, на котором будет выполнено обновление путей
    private int updateAtFrame = 0;  

    //  Массив узлов - создаётся один раз, при первом вызове скрипта
    private PathNode[,] grid = null;

    private void CheckWalkableNodes()
    {
        foreach (PathNode node in grid)
        {
            //  Пока что считаем все вершины проходимыми, без учёта препятствий
            node.walkable = true;
            node.walkable = !Physics.CheckSphere(node.body.transform.position, 1) && node.body.transform.position.y < 100f;
            if (node.walkable)
                node.Fade();
            else
            {
                node.Error();
                //Debug.Log("Not walkable!");
            }
        }
    }


    // Метод вызывается однократно перед отрисовкой первого кадра
    void Start()
    {
        //  Создаём сетку узлов для навигации - адаптивную, под размер ландшафта
        Vector3 terrainSize = landscape.terrainData.bounds.size;
        int sizeX = (int)(terrainSize.x / gridDelta);
        int sizeZ = (int)(terrainSize.z / gridDelta);
        //  Создаём и заполняем сетку вершин, приподнимая на 25 единиц над ландшафтом
        grid = new PathNode[sizeX,sizeZ];
        for (int x = 0; x < sizeX; ++x)
            for (int z = 0; z < sizeZ; ++z)
            {
                Vector3 position = new Vector3(x * gridDelta, 0, z * gridDelta);
                position.y = landscape.SampleHeight(position) + 25;
                grid[x, z] = new PathNode(nodeModel, false, position);
                grid[x, z].ParentNode = null;
                grid[x, z].Fade();
            }
    }
    /// <summary>
    /// Получение списка соседних узлов для вершины сетки
    /// </summary>
    /// <param name="current">индексы текущей вершины </param>
    /// <returns></returns>
    private List<Vector2Int> GetNeighbours(Vector2Int current)
    {
        List<Vector2Int> nodes = new List<Vector2Int>();
        for (int x = current.x - 1; x <= current.x + 1; ++x)
            for (int y = current.y - 1; y <= current.y + 1; ++y)
                if (x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1) && (x != current.x || y != current.y))
                    nodes.Add(new Vector2Int(x, y));
        return nodes;
    }

    /// <summary>
    /// Вычисление "кратчайшего" между двумя вершинами сетки
    /// </summary>
    /// <param name="startNode">Координаты начального узла пути (индексы элемента в массиве grid)</param>
    /// <param name="finishNode">Координаты конечного узла пути (индексы элемента в массиве grid)</param>
    void DijkstraPath(Vector2Int startNode, Vector2Int finishNode)
    {

        // Очищаем все узлы - сбрасываем отметку родителя, снимаем подсветку
        foreach (var node in grid)
        {
            node.Fade();
            node.ParentNode = null;
        }

        // На данный момент вызов этого метода не нужен, там только устанавливается проходимость вершины. Можно добавить обработку препятствий
        CheckWalkableNodes();

        // Реализуется аналог волнового алгоритма, причём найденный путь не будет являться оптимальным

        PathNode start = grid[startNode.x, startNode.y];

        // Начальную вершину отдельно изменяем
        start.ParentNode = null;
        start.Distance = 0;

        // Очередь вершин в обработке - в A* необходимо заменить на очередь с приоритетом
        var nodes = new SimplePriorityQueue<Vector2Int>();
        nodes.Enqueue(startNode, 0);

        // Пока не обработаны все вершины (очередь содержит узлы для обработки)
        while (nodes.Count() > 0)
        {
            Vector2Int current = nodes.Dequeue();
            // Если достали целевую - можно заканчивать (это верно и для A*)
            if (current == finishNode) break;
            // Получаем список соседей
            var neighbours = GetNeighbours(current);

            /* Менял код */
            foreach (var node in neighbours)
                if (grid[node.x, node.y].walkable && grid[node.x, node.y].Distance > grid[current.x, current.y].Distance + PathNode.Dist(grid[node.x, node.y], grid[current.x, current.y]))
                {
                    grid[node.x, node.y].ParentNode = grid[current.x, current.y];
                    if (nodes.Contains(node))
                        nodes.UpdatePriority(node, grid[current.x, current.y].Distance + PathNode.Dist(grid[node.x, node.y], grid[current.x, current.y]));
                    else
                        nodes.Enqueue(node, (grid[current.x, current.y].Distance + PathNode.Dist(grid[node.x, node.y], grid[current.x, current.y])));
                }
            grid[current.x, current.y].walkable = false;
        }
        // Восстанавливаем путь от целевой к стартовой
        var pathElem = grid[finishNode.x, finishNode.y];
        while (pathElem != null)
        {
            pathElem.Dijkstra();
            pathElem = pathElem.ParentNode;
        }
    }

    void AStarPath(Vector2Int startNode, Vector2Int finishNode)
    {

        // Очищаем все узлы - сбрасываем отметку родителя, снимаем подсветку
        foreach (var node in grid)
        {
            node.Fade();
            node.ParentNode = null;
        }

        // На данный момент вызов этого метода не нужен, там только устанавливается проходимость вершины. Можно добавить обработку препятствий
        CheckWalkableNodes();

        // Реализуется аналог волнового алгоритма, причём найденный путь не будет являться оптимальным

        PathNode start = grid[startNode.x, startNode.y];

        // Начальную вершину отдельно изменяем
        start.ParentNode = null;
        start.Distance = 0;

        // Очередь вершин в обработке - в A* необходимо заменить на очередь с приоритетом
        var nodes = new SimplePriorityQueue<Vector2Int>();
        nodes.Enqueue(startNode, 0);

        // Пока не обработаны все вершины (очередь содержит узлы для обработки)
        while (nodes.Count() > 0)
        {
            Vector2Int current = nodes.Dequeue();
            // Если достали целевую - можно заканчивать (это верно и для A*)
            //if (current == finishNode) break;
            // Получаем список соседей
            var neighbours = GetNeighbours(current);

            foreach (var node in neighbours)
            {
                var dist = grid[current.x, current.y].Distance + PathNode.Dist(grid[node.x, node.y], grid[current.x, current.y]) + PathNode.Dist(grid[node.x, node.y], grid[finishNode.x, finishNode.y]);
                if (grid[node.x, node.y].walkable && grid[node.x, node.y].Distance > dist)
                {
                    grid[node.x, node.y].ParentNode = grid[current.x, current.y];
                    if (nodes.Contains(node))
                        nodes.UpdatePriority(node, dist);
                    else
                        nodes.Enqueue(node, dist);
                }
            }
            grid[current.x, current.y].walkable = false;
        }
        // Восстанавливаем путь от целевой к стартовой
        var pathElem = grid[finishNode.x, finishNode.y];
        while (pathElem != null)
        {
            pathElem.AStar();
            pathElem = pathElem.ParentNode;
        }
    }

    // Метод вызывается каждый кадр
    void Update()
    {
        if (Input.GetKey(KeyCode.G))
            check = !check;
        //  Чтобы не вызывать этот метод каждый кадр, устанавливаем интервал вызова в 1000 кадров
        if (Time.frameCount < updateAtFrame) return;
        updateAtFrame = Time.frameCount + 100;

        if (check)
            DijkstraPath(new Vector2Int(0, 0), new Vector2Int(grid.GetLength(0) - 1, grid.GetLength(1) - 1));
        else
            AStarPath(new Vector2Int(0, 0), new Vector2Int(grid.GetLength(0) - 1, grid.GetLength(1) - 1));

        
    }
}
