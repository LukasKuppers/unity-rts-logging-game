using System;

public class Promise<PromisedT>
{
    private enum Status
    {
        PENDING, RESOLVED, REJECTED
    }

    private Status status;
    private PromisedT value;
    private Action<PromisedT> onResolved;
    private Action<string> onRejected;

    public Promise()
    {
        status = Status.PENDING;
        value = default(PromisedT);
    }

    /// <summary>
    /// Successfully settle the promise
    /// </summary>
    /// <param name="successValue">The return value of the promise</param>
    public Promise<PromisedT> Resolve(PromisedT successValue)
    {
        SafeResolve(successValue);
        return this;
    }

    public void SafeResolve(PromisedT successValue)
    {
        if (status == Status.PENDING)
        {
            status = Status.RESOLVED;
            value = successValue;
            onResolved?.Invoke(value);
        }
    }

    /// <summary>
    /// Settle the promise to rejection state
    /// </summary>
    public Promise<PromisedT> Reject()
    {
        return Reject("Default promise rejection message");
    }

    /// <summary>
    /// Settle the promise, reporting an error
    /// </summary>
    /// <param name="message">The error message</param>
    public Promise<PromisedT> Reject(string message)
    {
        SafeReject(message);
        return this;
    }

    public void SafeReject(string message)
    {
        if (status == Status.PENDING)
        {
            status = Status.REJECTED;
            onRejected?.Invoke(message);
        }
    }

    // callback doesn't take any args
    public Promise<PromisedT> Catch(Action callback)
    {
        return Catch((msg) => callback());
    }

    // callback takes error message as parameter
    public Promise<PromisedT> Catch(Action<string> callback)
    {
        Promise<PromisedT> promise = new Promise<PromisedT>();

        if (status == Status.PENDING)
        {
            onResolved = (PromisedT val) => promise.SafeResolve(val);
            onRejected = callback;
        } 
        else if (status == Status.RESOLVED)
            promise.SafeResolve(value);
        else
            callback("Default promise rejection message");

        return promise;
    }

    // callback doesn't take any args or return anything
    public Promise<bool> Then(Action callback)
    {
        return Then((PromisedT val) =>
        {
            callback();
            return true;
        });
    }

    // callback doesn't return anything
    public Promise<bool> Then(Action<PromisedT> callback)
    {
        return Then((PromisedT val) =>
        {
            callback(val);
            return true;
        });
    }

    // callback returns value but is not async
    public Promise<ConvertT> Then<ConvertT>(Func<PromisedT, ConvertT> callback)
    {
        return InternalReturningThen((PromisedT val, Promise<ConvertT> promise) =>
        {
            ConvertT callbackValue = callback(val);
            promise.SafeResolve(callbackValue);
        });
    }

    // callback is async - returns a promise
    public Promise<ConvertT> Then<ConvertT>(Func<PromisedT, Promise<ConvertT>> callback)
    {
        return InternalReturningThen((PromisedT val, Promise<ConvertT> promise) =>
        {
            Promise<ConvertT> callbackPromise = callback(val);
            callbackPromise
                .Then((ConvertT callbackVal) => promise.SafeResolve(callbackVal))
                .Catch((string msg) => promise.SafeReject(msg));
        });
    }

    // callback is async - returns a promise, but doesnt take any arguments
    public Promise<ConvertT> Then<ConvertT>(Func<Promise<ConvertT>> callback)
    {   
        return InternalReturningThen((PromisedT _, Promise<ConvertT> promise) =>
        {
            Promise<ConvertT> callbackPromise = callback();
            callbackPromise
                .Then((ConvertT callbackVal) => promise.SafeResolve(callbackVal))
                .Catch((string msg) => promise.SafeReject(msg));
        });
    }

    private Promise<ConvertT> InternalReturningThen<ConvertT>(Action<PromisedT, Promise<ConvertT>> initPromiseResolution)
    {
        Promise<ConvertT> promise = new Promise<ConvertT>();

        if (status == Status.PENDING)
        {
            onResolved = (PromisedT val) => initPromiseResolution(val, promise);
            onRejected = (string msg) => promise.SafeReject(msg);
        }
        else if (status == Status.RESOLVED)
            initPromiseResolution(value, promise);
        else
            promise.Reject();

        return promise;
    }
}
