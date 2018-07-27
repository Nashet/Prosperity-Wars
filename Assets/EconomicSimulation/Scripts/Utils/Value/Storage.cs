using System;
using Nashet.EconomicSimulation;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using UnityEngine;

namespace Nashet.ValueSpace
{
    public class Storage : Value, IClickable, ICopyable<Storage>
    {
        public static readonly Storage EmptyProduct = new Storage(Product.Grain, 0f);

        private Product product;

        public bool IsStorabe { get { return Product.IsStorable; } }

        // protected  Value value;
        //public Value value;
        //public Storage(JSONObject jsonObject)
        //{
        //    //  Auto-generated constructor stub
        //}
        public Storage(Product inProduct, float inAmount, bool showMessageAboutNegativeValue = true) : base(inAmount, showMessageAboutNegativeValue)
        {
            product = inProduct;
            //value = new Value(inAmount);
            // TODO exceptions!!
        }

        public Storage(Product inProduct, ReadOnlyValue inAmount) : base(inAmount)
        {
            product = inProduct;
        }

        public Storage(Product product) : this(product, 0f)
        {
        }

        public Storage(Storage storage) : this(storage.Product, storage)
        {
        }

        //public static int CostOrder(Storage x, Storage y)
        //{
        //    //eats less memory
        //    float sumX = x.get() * (float)Country.market.getCost(x.Product).Get();
        //    float sumY = y.get() * (float)Country.market.getCost(y.Product).Get();
        //    return sumX.CompareTo(sumY);

        //    //return Country.market.getCost(x).get().CompareTo(Country.market.getCost(y).get());
        //}

        public void set(Product inProduct, float inAmount, bool showMessageAboutNegativeValue = true)
        {
            product = inProduct;
            Set(inAmount, showMessageAboutNegativeValue);
        }

        public void set(Storage storage)
        {
            product = storage.Product;
            Set(storage);
        }

        //[System.Obsolete("Method is deprecated, need product specified")]
        //override public void set(Value invalue)
        //{
        //    throw new DontUseThatMethod();
        //}
        //[System.Obsolete("Method is deprecated, need product specified")]
        //override public void set(float inAmount, bool showMessageAboutOperationFails = true)
        //{
        //    // need product specified
        //    throw new DontUseThatMethod();
        //}
        //[System.Obsolete("Method is deprecated, need product specified")]
        //override public void add(Value invalue, bool showMessageAboutNegativeValue = true)
        //{
        //    throw new DontUseThatMethod();
        //}
        [Obsolete("Method is deprecated, need product specified")]
        public Storage add(float invalue, bool showMessageAboutNegativeValue = true)
        {
            Add(invalue, showMessageAboutNegativeValue);
            //throw new DontUseThatMethod(); temporally
            return this;
        }

        public void add(Storage storage, bool showMessageAboutNegativeValue = true)
        {
            if (isExactlySameProduct(storage))
                Add(storage, showMessageAboutNegativeValue);
            else
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Attempt to add wrong product to Storage");
            }
        }

        public Product Product
        {
            get { return product; }
        }

        public string ToStringWithoutSubstitutes()
        {
            return get() + " " + Product.ToStringWithoutSubstitutes();
        }

        public override string ToString()
        {
            return get().ToString("N3") + " " + Product;
        }

        public void sendAll(StorageSet whom)
        {
            send(whom, this);
        }

        public void sendAll(Storage another)
        {
            if (!isExactlySameProduct(another))
                Debug.Log("Attempt to give wrong product");
            else
            {
                another.add(this);
                SetZero();
            }
        }

        /// <summary>
        /// checks inside (duplicates?)
        /// </summary>
        public void send(StorageSet whom, Storage howMuch)
        {
            if (has(howMuch))
            {
                Storage targetStorage = new Storage(howMuch);
                whom.Add(targetStorage);
                subtract(howMuch);
            }
        }

        /// <summary>
        /// checks inside (duplicates?),
        /// </summary>
        //public void send(Storage another, float amount)
        //{
        //    if (this.Product != another.Product)
        //        Debug.Log("Attempt to give wrong product");
        //    else
        //        base.send(another, amount);
        //}

        /// <summary>
        /// checks inside (duplicates?), returns true if succeeded
        /// </summary>
        public bool send(Storage reciever, Storage amountToSend, bool showMessageAboutOperationFails = true)
        {
            if (!isExactlySameProduct(reciever))
            {
                Debug.Log("Attempt to give wrong product");
                return false;
            }
            else
            {
                if (isBiggerOrEqual(amountToSend))
                {
                    subtract(amountToSend);
                    reciever.add(amountToSend);
                    return true;
                }
                else
                {
                    if (showMessageAboutOperationFails)
                        Debug.Log("No enough value to send");
                    sendAll(reciever);
                    return false;
                }
            }
        }

        public Storage Multiply(float invalue, bool showMessageAboutOperationFails = true)
        {
            //if (invalue < 0f)
            //{
            //    if (showMessageAboutOperationFails)
            //        Debug.Log("Storage multiply failed");
            //    return new Storage(this.Product, 0f);
            //}
            //else
            //    return new Storage(this.Product, get() * invalue);
            base.Multiply(invalue, showMessageAboutOperationFails);
            return this;
        }

        /// <summary>
        /// returns new value
        /// </summary>
        public Storage Multiply(ReadOnlyValue invalue)
        {
            //return new Storage(this.Product, get() * invalue.get());
            base.Multiply(invalue);
            return this;
        }

        /// <summary> Returns true if has that good or it's substitute</summary>
        public bool has(Storage storage)
        {
            if (!isSameProductType(storage))
            {
                // Debug.Log("Attempted to pay wrong product!");
                return false;
            }
            else
                return isBiggerOrEqual(storage);
        }

        //public bool hasSubstitute(Storage storage)
        //{
        //    if (!isSubstituteProduct(storage.Product))
        //    {
        //        // Debug.Log("Attempted to pay wrong product!");
        //        return false;
        //    }
        //    else
        //        return isBiggerOrEqual(storage);
        //}

        /// <summary> Returns true if products exactly same or this is substitute for anotherStorage</summary>
        public bool isSameProductType(Storage anotherStorage)
        {
            return Product.isSameProduct(anotherStorage.Product);
        }

        /// <summary> Returns true if products exactly same or this is substitute for anotherProduct</summary>
        public bool isSameProductType(Product anotherProduct)
        {
            return Product.isSameProduct(anotherProduct);
        }

        /// <summary> Returns true only if products exactly same. Does not coiunt substitutes</summary>
        public bool isExactlySameProduct(Storage anotherStorage)
        {
            return Product == anotherStorage.Product;
        }

        /// <summary> Returns true only if products exactly same. Does not count substitutes</summary>
        public bool isExactlySameProduct(Product anotherProduct)
        {
            return Product == anotherProduct;
        }

        //public bool isSubstituteProduct(Product product)
        //{
        //    return this.Product.isSubstituteFor(product);
        //}
        public bool isAbstractProduct()
        {
            return Product.isAbstract();
        }

        //[System.Obsolete("Method is deprecated, need product specified")]
        //override public Value multipleOutside(float invalue, bool showMessageAboutOperationFails = true)
        //{
        //    throw new DontUseThatMethod();
        //}
        //[System.Obsolete("Method is deprecated, need product specified")]
        //override public Value multipleOutside(Value invalue, bool showMessageAboutNegativeValue = true)
        //{
        //    throw new DontUseThatMethod();
        //}
        public Storage subtract(Storage storage, bool showMessageAboutNegativeValue = true)
        {
            //if (!this.isSameProductType(storage.Product))
            if (!storage.isSameProductType(Product))
            {
                Debug.Log("Storage subtract Outside failed - wrong product");
                Set(0f);
            }
            else if (storage.isBiggerThan(this))
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Storage subtract Outside failed");
                Set(0f);
            }
            else
                Set(get() - storage.get());
            return this;
        }

        public void OnClicked()
        {
            if (!isAbstractProduct())
                MainCamera.tradeWindow.selectProduct((this).Product);
        }

        public Storage Copy()
        {
            return new Storage(this);
        }
    }
}