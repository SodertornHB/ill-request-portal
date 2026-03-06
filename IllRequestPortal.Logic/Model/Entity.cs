
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IllRequestPortal.Logic.Model
{
    public class Entity
    {
        public virtual int Id { get; set; }

        public override bool Equals(object obj)
        {
            var castedObj = obj as Entity;
            if (castedObj == null) throw new ArgumentException(obj.GetType().Name);
            if (BothEntitiesAreNew(castedObj)) return false;
            return Id == castedObj.Id;
        }

        public override int GetHashCode()
        {
                
          return Id.GetHashCode();
        }        

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        private bool BothEntitiesAreNew(Entity castedObj)
        {
                
            return Id == 0 && castedObj.Id == 0;
                    
        }
    }

    public class EntityComparer : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y) => x.Id == y.Id;

        public int GetHashCode(Entity obj) => obj.Id.GetHashCode();
    }

    public static class EnityComparerExtensions
    {
        public static IEnumerable<T> ExceptFor<T>(this IEnumerable<T> list, IEnumerable<Entity> compareList) where T : Entity
        { 
            return list.Except(compareList, new EntityComparer()).Cast<T>();
        }
    }
}