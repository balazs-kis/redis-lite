using System;

namespace RedisLite.TestHelpers
{
    public sealed class Arranged
    {
        public Acted Act(Action actAction) =>
            new Acted(CatchException(actAction));

        public Acted<TResult> Act<TResult>(Func<TResult> actFunc) =>
            new Acted<TResult>(CatchException(actFunc));

        private ActResult CatchException(Action action)
        {
            try
            {
                action.Invoke();
                return ActResult.Ok();
            }
            catch (Exception ex)
            {
                return ActResult.ThrewException(ex);
            }
        }

        private ActResult<T> CatchException<T>(Func<T> func)
        {
            T result;

            try
            {
                result = func.Invoke();
            }
            catch (Exception ex)
            {
                return ActResult.ThrewException<T>(ex);
            }

            return ActResult.Ok(result);
        }
    }

    public sealed class Arranged<T>
    {
        private readonly T _underTest;

        public Arranged(T underTest)
        {
            _underTest = underTest;
        }

        public Acted Act(Action<T> actAction) =>
            new Acted(CatchException(actAction));

        public Acted<TResult> Act<TResult>(Func<T, TResult> actFunc) =>
            new Acted<TResult>(CatchException(actFunc));

        private ActResult CatchException(Action<T> func)
        {
            try
            {
                func.Invoke(_underTest);
                return ActResult.Ok();
            }
            catch (Exception ex)
            {
                return ActResult.ThrewException(ex);
            }
        }

        private ActResult<TOut> CatchException<TOut>(Func<T, TOut> func)
        {
            TOut result;

            try
            {
                result = func.Invoke(_underTest);
            }
            catch (Exception ex)
            {
                return ActResult.ThrewException<TOut>(ex);
            }

            return ActResult.Ok(result);
        }
    }

    public sealed class Arranged<T, TParam>
    {
        private readonly T _underTest;
        private readonly TParam _parameter;

        public Arranged(T underTest, TParam parameter)
        {
            _underTest = underTest;
            _parameter = parameter;
        }

        public Acted Act(Action<T, TParam> actAction) =>
            new Acted(CatchException(actAction));

        public Acted<TResult> Act<TResult>(Func<T, TParam, TResult> actFunc) =>
            new Acted<TResult>(CatchException(actFunc));

        private ActResult CatchException(Action<T, TParam> action)
        {
            try
            {
                action.Invoke(_underTest, _parameter);
                return ActResult.Ok();
            }
            catch (Exception ex)
            {
                return ActResult.ThrewException(ex);
            }
        }

        private ActResult<TOut> CatchException<TOut>(Func<T, TParam, TOut> func)
        {
            TOut result;

            try
            {
                result = func.Invoke(_underTest, _parameter);
            }
            catch (Exception ex)
            {
                return ActResult.ThrewException<TOut>(ex);
            }

            return ActResult.Ok(result);
        }
    }

    public sealed class Arranged<T, TParam1, TParam2>
    {
        private readonly T _underTest;
        private readonly TParam1 _parameter1;
        private readonly TParam2 _parameter2;

        public Arranged(T underTest, TParam1 parameter1, TParam2 parameter2)
        {
            _underTest = underTest;
            _parameter1 = parameter1;
            _parameter2 = parameter2;
        }

        public Acted Act(Action<T, TParam1, TParam2> actAction) =>
            new Acted(CatchException(actAction));

        public Acted<TResult> Act<TResult>(Func<T, TParam1, TParam2, TResult> actFunc) =>
            new Acted<TResult>(CatchException(actFunc));

        private ActResult CatchException(Action<T, TParam1, TParam2> action)
        {
            try
            {
                action.Invoke(_underTest, _parameter1, _parameter2);
                return ActResult.Ok();
            }
            catch (Exception ex)
            {
                return ActResult.ThrewException(ex);
            }
        }

        private ActResult<TOut> CatchException<TOut>(Func<T, TParam1, TParam2, TOut> func)
        {
            TOut result;

            try
            {
                result = func.Invoke(_underTest, _parameter1, _parameter2);
            }
            catch (Exception ex)
            {
                return ActResult.ThrewException<TOut>(ex);
            }

            return ActResult.Ok(result);
        }
    }
}