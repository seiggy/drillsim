using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DrillingPhysicalQuantity : BasePhysicalQuantity
    {
        private static List<BasePhysicalQuantity> availablePhysicalQuantities_ = null;
        internal static Dictionary<string, BasePhysicalQuantity> physicalQuantitiesByName_ = null;
        internal static Dictionary<Guid, BasePhysicalQuantity> physicalQuantitiesByGuid_ = null;
        public static List<BasePhysicalQuantity> AvailablePhysicalQuantities
        {
            get
            {
                if (availablePhysicalQuantities_ == null)
                {
                    Initialize();
                }
                return availablePhysicalQuantities_;
            }
        }

        private static void Initialize()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(DrillingPhysicalQuantity));
            if (assembly != null)
            {
                foreach (Type typ in assembly.GetTypes())
                {
                    if (typ.IsSubclassOf(typeof(BasePhysicalQuantity)))
                    {
                        MethodInfo method = null;
                        foreach (MethodInfo meth in typ.GetMethods())
                        {
                            if (meth.IsStatic &&
                                meth.Name.EndsWith("Instance") &&
                                meth.ReturnType.IsSubclassOf(typeof(BasePhysicalQuantity)))
                            {
                                method = meth;
                                break;
                            }
                        }
                        // call the method
                        if (method != null)
                        {
                            object obj = method.Invoke(null, null);
                            if (obj != null)
                            {
                                var res = (BasePhysicalQuantity)obj;
                                if (availablePhysicalQuantities_ == null)
                                {
                                    availablePhysicalQuantities_ = new List<BasePhysicalQuantity>();
                                }
                                availablePhysicalQuantities_.Add(res);
                                if (physicalQuantitiesByGuid_ == null)
                                {
                                    physicalQuantitiesByGuid_ = new Dictionary<Guid, BasePhysicalQuantity>();
                                }
                                if (res.ID != Guid.Empty && !physicalQuantitiesByGuid_.ContainsKey(res.ID))
                                {
                                    physicalQuantitiesByGuid_.Add(res.ID, res);
                                }
                                else
                                {
                                    throw new Exception("problem with ID of drilling physical quantity");
                                }
                                if (physicalQuantitiesByName_ == null)
                                {
                                    physicalQuantitiesByName_ = new Dictionary<string, BasePhysicalQuantity>(StringComparer.OrdinalIgnoreCase);
                                }
                                string lookupName = NormalizeQuantityLookupName(res.Name);
                                if (!string.IsNullOrEmpty(lookupName) && !physicalQuantitiesByName_.ContainsKey(lookupName))
                                {
                                    physicalQuantitiesByName_.Add(lookupName, res);
                                }
                                else
                                {
                                    throw new Exception("problem with name of drilling physical quantity");
                                }
                            }
                        }
                    }
                }
                RegisterUniqueSynonyms(availablePhysicalQuantities_, physicalQuantitiesByName_);
            }
        }

        public static BasePhysicalQuantity GetQuantity(DrillingPhysicalQuantity.QuantityEnum choice)
        {
            BasePhysicalQuantity quantity = null;
            Guid guid;

            if (enumLookUp_.TryGetValue(choice, out guid))
            {
                if (physicalQuantitiesByGuid_ == null)
                {
                    Initialize();
                }
                physicalQuantitiesByGuid_.TryGetValue(guid, out quantity);
            }
            return quantity;
        }
        public static new BasePhysicalQuantity GetQuantity(Guid choiceID)
        {
            BasePhysicalQuantity quantity = null;
            if (physicalQuantitiesByGuid_ == null)
            {
                Initialize();
            }
            physicalQuantitiesByGuid_.TryGetValue(choiceID, out quantity);
            if (quantity == null)
            {
                quantity = BasePhysicalQuantity.GetQuantity(choiceID);
            }
            return quantity;
        }

        public static new BasePhysicalQuantity GetQuantity(string choiceName)
        {
            BasePhysicalQuantity quantity = null;
            if (physicalQuantitiesByName_ == null)
            {
                Initialize();
            }
            physicalQuantitiesByName_.TryGetValue(NormalizeQuantityLookupName(choiceName), out quantity);
            if (quantity == null)
            {
                quantity = BasePhysicalQuantity.GetQuantity(choiceName);
            }
            return quantity;
        }

    }
}
