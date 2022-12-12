// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Globalization;

namespace Vlingo.Xoom.Actors;

public interface IMailboxConfiguration<out T>
{
    T WithName(string name);
    
    T WithClassname(string classname);
    
    T WithDefaultMailbox(bool flag);
}

public interface IArrayQueue : IMailboxConfiguration<IArrayQueue>
{
    IArrayQueue Size(int size);
    
    IArrayQueue FixedBackoff(int amount);
    
    IArrayQueue NotifyOnSend(bool flag);
    
    IArrayQueue DispatcherThrottlingCount(int count);
    
    IArrayQueue SendRetires(int retries);
}

public interface IConcurrentQueue : IMailboxConfiguration<IConcurrentQueue>
{
    IConcurrentQueue NumberOfDispatchersFactor(double factor);
    
    IConcurrentQueue NumberOfDispatchers(int dispatchers);
  
    IConcurrentQueue DispatcherThrottlingCount(int count);
}

public interface ISharedRingBuffer : IMailboxConfiguration<ISharedRingBuffer>
{
    ISharedRingBuffer Size(int size);
    
    ISharedRingBuffer FixedBackoff(int amount);
    
    ISharedRingBuffer NotifyOnSend(bool flag);
    
    ISharedRingBuffer DispatcherThrottlingCount(int count);
}

  //=========================================
  // Implementations
  //=========================================

public abstract class BaseMailboxConfiguration<T> : IMailboxConfiguration<T>
{
    protected string? Name;
    protected Properties? Properties;
    
    public T WithName(string name)
    {
        Properties = new Properties();
        Name = "plugin.name." + name;
        Properties.SetProperty(Name, "true");

        return (T)(object) this;
    }
    
    public T WithClassname(string classname)
    {
        Properties?.SetProperty(Name + ".classname", "");

        return (T)(object) this;
    }
    
    public T WithDefaultMailbox(bool flag)
    {
        Properties?.SetProperty(Name + ".defaultMailbox", flag.ToString());

        return (T)(object) this;
    }
}

public class ArrayQueueConfiguration : BaseMailboxConfiguration<IArrayQueue>, IArrayQueue
{
    public IArrayQueue Size(int size)
    {
        Properties?.SetProperty(Name + ".size", size.ToString());

        return this;
    }

    public IArrayQueue FixedBackoff(int amount)
    {
        Properties?.SetProperty(Name + ".fixedBackoff", amount.ToString());

        return this;
    }

    public IArrayQueue NotifyOnSend(bool flag)
    {
          Properties?.SetProperty(Name + ".notifyOnSend", flag.ToString());

          return this;
    }

    public IArrayQueue DispatcherThrottlingCount(int count)
    {
          Properties?.SetProperty(Name + ".dispatcherThrottlingCount", count.ToString());

          return this;
    }

    public IArrayQueue SendRetires(int retries)
    {
          Properties?.SetProperty(Name + ".sendRetires", retries.ToString());

          return this;
    }
}
  
public class ConcurrentQueueConfiguration : BaseMailboxConfiguration<IConcurrentQueue>, IConcurrentQueue
{
    public IConcurrentQueue NumberOfDispatchersFactor(double factor)
    {
          Properties?.SetProperty(Name + ".numberOfDispatchersFactor", factor.ToString(CultureInfo.InvariantCulture));

          return this;
    }

    public IConcurrentQueue NumberOfDispatchers(int dispatchers)
    {
          Properties?.SetProperty(Name + ".numberOfDispatchers", dispatchers.ToString());

          return this;
    }

    public IConcurrentQueue DispatcherThrottlingCount(int count)
    {
          Properties?.SetProperty(Name + ".dispatcherThrottlingCount", count.ToString());

          return this;
    }
}

public class SharedRingBufferConfiguration : BaseMailboxConfiguration<ISharedRingBuffer>, ISharedRingBuffer
{
    public ISharedRingBuffer Size(int size)
    {
        Properties?.SetProperty(Name + ".size", size.ToString());

        return this;
    }

    public ISharedRingBuffer FixedBackoff(int amount)
    {
        Properties?.SetProperty(Name + ".fixedBackoff", amount.ToString());

        return this;
    }

    public ISharedRingBuffer NotifyOnSend(bool flag)
    {
        Properties?.SetProperty(Name + ".notifyOnSend", flag.ToString());

      return this;
    }

    public ISharedRingBuffer DispatcherThrottlingCount(int count)
    {
        Properties?.SetProperty(Name + ".dispatcherThrottlingCount", count.ToString());

        return this;
    }
}