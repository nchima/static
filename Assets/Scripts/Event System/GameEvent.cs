using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent {
    public delegate void Handler(GameEvent e);
}