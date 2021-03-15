using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEvents {
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

    public class PlayerUsedSpecialMove : GameEvent {
        public PlayerUsedSpecialMove() { }
    }

    public class PlayerUsedFallThroughFloorMove : GameEvent {
        public PlayerUsedFallThroughFloorMove() {}
    }

    public class PlayerLookedDown : GameEvent {
        public PlayerLookedDown() {}
    }

    public class PlayerFellOutOfLevel : GameEvent {
        public PlayerFellOutOfLevel() {}
    }

    public class FallingSequenceStarted : GameEvent {
        public FallingSequenceStarted() {}
    }

    public class PlayerLanded : GameEvent {
        public PlayerLanded() {}
    }

    public class FallingSequenceFinished : GameEvent {
        public FallingSequenceFinished() {}
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

    public class PlayerWasTased : GameEvent {
        public PlayerWasTased() { }
    }

    public class PickupObtained : GameEvent {
        public PickupObtained() { }
    }

    public class Bullseye : GameEvent {
        public Bullseye() { }
    }

    public class LevelLoaded : GameEvent {
        public LevelLoaded() {}
    }

    public class EnableFeet : GameEvent {
        public bool value;
        public EnableFeet(bool value) {
            this.value = value;
        }
    }

    public class NowEnteringScreenActivated : GameEvent {
        public bool active;
        public NowEnteringScreenActivated(bool active) {
            this.active = active;
        }
    }
}