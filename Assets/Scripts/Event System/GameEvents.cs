using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEvents {
    public class EnemyDied : GameEvent {
        public EnemyDied() { }
    }

    public class GameStarted : GameEvent {
        public GameStarted() { }
    }

    public class GameOver : GameEvent {
        public GameOver() { }
    }

    public class LevelCompleted : GameEvent {
        public LevelCompleted() { }
    }

    public class PlayerFiredGun : GameEvent {
        public PlayerFiredGun() { }
    }

    public class PlayerKilledEnemy : GameEvent {
        public int scoreValue;
        public float specialValue;
        public PlayerKilledEnemy(int scoreValue, float specialValue) {
            this.scoreValue = scoreValue;
            this.specialValue = specialValue;
        }
    }

    public class PlayerWasHurt : GameEvent {
        public PlayerWasHurt() { }
    }
}