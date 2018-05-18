// Copyright 2012-2018 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Authority
{
    using System.Collections.Generic;


    public sealed class FactHandleIdEqualityComparer :
        IEqualityComparer<FactHandle>
    {
        public static IEqualityComparer<FactHandle> Shared { get; } = new FactHandleIdEqualityComparer();

        public bool Equals(FactHandle x, FactHandle y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            return x.FactId == y.FactId;
        }

        public int GetHashCode(FactHandle obj)
        {
            return obj.FactId.GetHashCode();
        }
    }
}