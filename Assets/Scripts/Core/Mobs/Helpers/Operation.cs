using System;

namespace Assets.Scripts.Core.Mobs.Helpers
{
    public class Operation
    {
        private float mTimer;
        private float mTime;

        private Action<Operation> mOnComplete;
        private Action<Operation> mOnProcess;
        private Action<Operation> mOnAbort;

        public float Time => mTime;
        public float Timer => mTimer;

        public Action<Operation> OnComplete
        {
            set => mOnComplete = value;
        }
        public Action<Operation> OnProcess
        {
            set => mOnProcess = value;
        }
        public Action<Operation> OnAbort
        {
            set => mOnAbort = value;
        }

        public void Execute(float time)
        {
            mTime = time;
           
            mTimer = 0.0f;
            InProcess = true;
        }

        public virtual void Process(float dTime)
        {
            if (InProcess)
            {
                mTimer += dTime;

                InnerProcess(dTime);

                if (mOnProcess != null)
                    mOnProcess(this);

                if (mTimer > mTime)
                {
                    InProcess = false;

                    InnerComplete();

                    if (mOnComplete != null)
                        mOnComplete(this);
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
                if (mTime <= 0)
                    return 0.0f;

                return mTimer/mTime;
            }
        }
        public virtual void Abort()
        {
            if (mOnAbort != null)
                mOnAbort(this);
        }

        protected void ForceComplete()
        {
            InProcess = false;

            InnerComplete();

            if (mOnComplete != null)
                mOnComplete(this);
        }
    }
}