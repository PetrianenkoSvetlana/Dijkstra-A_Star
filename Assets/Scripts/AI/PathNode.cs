﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode //: MonoBehaviour
{
    public bool walkable;           //  Свободна для перемещения
    public Vector3 worldPosition;   //  Позиция в глобальных координатах
    private GameObject objPrefab;   //  Шаблон объекта
    public GameObject body;         //  Объект для отрисовки
    
    private PathNode parentNode = null;               //  откуда пришли
    
    /// <summary>
    /// Родительская вершина - предшествующая текущей в пути от начальной к целевой
    /// </summary>
    public PathNode ParentNode
    {
        get => parentNode;
        set => SetParent(value);
    }

    private float distance = float.PositiveInfinity;  //  расстояние от начальной вершины

    /// <summary>
    /// Расстояние от начальной вершины до текущей (+infinity если ещё не развёртывали)
    /// </summary>
    public float Distance
    {
        get => distance;
        set => distance = value;
    }

    /// <summary>
    /// Устанавливаем родителя и обновляем расстояние от него до текущей вершины. Неоптимально - дважды расстояние считается
    /// </summary>
    /// <param name="parent"></param>
    private void SetParent(PathNode parent)
    {
        //  Указываем родителя
        parentNode = parent;
        //  Вычисляем расстояние
        if (parent != null)
            distance = parent.Distance + PathNode.Dist(parentNode, this);
        else
            distance = float.PositiveInfinity;
    }

    /// <summary>
    /// Конструктор вершины
    /// </summary>
    /// <param name="_objPrefab">объект, который визуализируется в вершине</param>
    /// <param name="_walkable">проходима ли вершина</param>
    /// <param name="position">мировые координаты</param>
    public PathNode(GameObject _objPrefab, bool _walkable, Vector3 position)
    {
        objPrefab = _objPrefab;
        walkable = _walkable;
        worldPosition = position;
        body = GameObject.Instantiate(objPrefab, worldPosition, Quaternion.identity);
    }

    /// <summary>
    /// Расстояние между вершинами - разброс по высоте учитывается дополнительно
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Dist(PathNode sourse, PathNode dest)
    {
        var baseDist = Vector3.Distance(sourse.body.transform.position, dest.body.transform.position);
        if (dest.body.transform.position.y > sourse.body.transform.position.y)
            return baseDist + 40 * (dest.body.transform.position.y - sourse.body.transform.position.y);
        else
            return baseDist;
    }
    
    /// <summary>
    /// Подсветить вершину - перекрасить в красный
    /// </summary>
    public void Dijkstra()
    {
        body.GetComponent<Renderer>().material.color = Color.red;
    }
    
    /// <summary>
    /// Снять подсветку с вершины - перекрасить в синий
    /// </summary>
    public void Fade()
    {
        body.GetComponent<Renderer>().material.color = Color.blue;
    }
    public void Error()
    {
        body.GetComponent<Renderer>().material.color = Color.gray;
    }
    public void AStar()
    {
        body.GetComponent<Renderer>().material.color = Color.green;
    }
}
