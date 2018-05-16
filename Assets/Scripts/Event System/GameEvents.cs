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
        public Enemy enemyKilled;
        public PlayerKilledEnemy(int scoreValue, float specialValue, Enemy enemyKilled) {
            this.scoreValue = scoreValue;
            this.specialValue = specialValue;
            this.enemyKilled = enemyKilled;
        }
    }

    public class PlayerWasHurt : GameEvent {
        public PlayerWasHurt() { }
    }
}