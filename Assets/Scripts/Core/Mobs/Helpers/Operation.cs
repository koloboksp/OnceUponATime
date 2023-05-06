using System;

namespace Assets.Scripts.Core.Mobs.Helpers
{
    public class Operation
    {
        private float _timer;
        private float _time;

        private Action<Operation> _onComplete;
        private Action<Operation> _onProcess;
        private Action<Operation> _onAbort;

        public float Time => _time;
        public float Timer => _timer;

        public Action<Operation> OnComplete
        {
            set => _onComplete = value;
        }
        public Action<Operation> OnProcess
        {
            set => _onProcess = value;
        }
        public Action<Operation> OnAbort
        {
            set => _onAbort = value;
        }

        public void Execute(float time)
        {
            _time = time;
           
            _timer = 0.0f;
            InProcess = true;
        }

        public virtual void Process(float dTime)
        {
            if (InProcess)
            {
                _timer += dTime;

                InnerProcess(dTime);

                if (_onProcess != null)
                    _onProcess(this);

                if (_timer > _time)
                {
                    InProcess = false;

                    InnerComplete();

                    if (_onComplete != null)
                        _onComplete(this);
                }
            }
        }
        
        protected virtual void InnerProcess(float dTime) { }
        
        protected virtual void InnerComplete() { }

        public bool InProcess { get; private set; }

        public float NormElapsedTime
        {
            get
            {
                if (_time <= 0)
                    return 0.0f;

                return _timer/_time;
            }
        }
        
        public virtual void Abort()
        {
            if (_onAbort != null)
                _onAbort(this);
        }

        protected void ForceComplete()
        {
            InProcess = false;

            InnerComplete();

            if (_onComplete != null)
                _onComplete(this);
        }
    }
}