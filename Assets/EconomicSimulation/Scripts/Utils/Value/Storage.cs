using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Nashet.EconomicSimulation;
using Nashet.Utils;
using Nashet.UnityUIUtils;
namespace Nashet.ValueSpace
{
    public class Storage : Value, IClickable, ICopyable<Storage>
    {
        static public readonly Storage EmptyProduct = new Storage(Product.Grain, 0f);

        private Product product;
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
        public Storage(Product inProduct, Value inAmount) : base(inAmount)
        {
            product = inProduct;
        }

        public Storage(Product product) : this(product, 0f)
        {

        }
        public Storage(Storage storage) : this(storage.getProduct(), storage)
        {

        }
        static public int CostOrder(Storage x, Storage y)
        {
            //eats less memory
            float sumX = x.get() * Game.market.getPrice(x.getProduct()).get();
            float sumY = y.get() * Game.market.getPrice(y.getProduct()).get();
            return sumX.CompareTo(sumY);

            //return Game.market.getCost(x).get().CompareTo(Game.market.getCost(y).get());
        }
        public void set(Product inProduct, float inAmount, bool showMessageAboutNegativeValue = true)
        {
            product = inProduct;
            set(inAmount, showMessageAboutNegativeValue);
        }
        public void set(Storage storage)
        {
            product = storage.getProduct();
            base.set(storage);
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
        [System.Obsolete("Method is deprecated, need product specified")]
        public Storage add(float invalue, bool showMessageAboutNegativeValue = true)
        {
            base.add(invalue, showMessageAboutNegativeValue);
            //throw new DontUseThatMethod(); temporally
            return this;
        }
        public void add(Storage storage, bool showMessageAboutNegativeValue = true)
        {
            if (this.isExactlySameProduct(storage))
                base.Add(storage, showMessageAboutNegativeValue);
            else
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Attempt to add wrong product to Storage");
            }
        }

        public Product getProduct()
        {
            return product;
        }
        public string ToStringWithoutSubstitutes()
        {
            return get() + " " + getProduct().ToStringWithoutSubstitutes();
        }
        override public string ToString()
        {
            return get() + " " + getProduct();

        }
        public void sendAll(StorageSet whom)
        {
            this.send(whom, this);
        }
        public void sendAll(Storage another)
        {
            if (!isExactlySameProduct(another))
                Debug.Log("Attempt to give wrong product");
            else
            {
                another.add(this);
                this.setZero();
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
                this.subtract(howMuch);
            }
        }
        /// <summary>
        /// checks inside (duplicates?),
        /// </summary>    
        //public void send(Storage another, float amount)
        //{
        //    if (this.getProduct() != another.getProduct())
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
                if (this.isBiggerOrEqual(amountToSend))
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


        internal Storage multiplyOutside(float invalue, bool showMessageAboutOperationFails = true)
        {
            if (invalue < 0f)
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Storage multiply failed");
                return new Storage(this.getProduct(), 0f);
            }
            else
                return new Storage(this.getProduct(), get() * invalue);
        }
        /// <summary>
        /// returns new value
        /// </summary>    
        public Storage multiplyOutside(Value invalue)
        {
            return new Storage(this.getProduct(), get() * invalue.get());
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
        //    if (!isSubstituteProduct(storage.getProduct()))
        //    {
        //        // Debug.Log("Attempted to pay wrong product!");
        //        return false;
        //    }
        //    else
        //        return isBiggerOrEqual(storage);
        //}

        /// <summary> Returns true if products exactly same or this is substitute for anotherStorage</summary>    
        internal bool isSameProductType(Storage anotherStorage)
        {
            return this.getProduct().isSameProduct(anotherStorage.getProduct());
        }
        /// <summary> Returns true if products exactly same or this is substitute for anotherProduct</summary>
        public bool isSameProductType(Product anotherProduct)
        {
            return this.getProduct().isSameProduct(anotherProduct);
        }
        /// <summary> Returns true only if products exactly same. Does not coiunt substitutes</summary>    
        internal bool isExactlySameProduct(Storage anotherStorage)
        {
            return this.getProduct() == anotherStorage.getProduct();
        }
        /// <summary> Returns true only if products exactly same. Does not count substitutes</summary>    
        public bool isExactlySameProduct(Product anotherProduct)
        {
            return this.getProduct() == anotherProduct;
        }

        //internal bool isSubstituteProduct(Product product)
        //{
        //    return this.getProduct().isSubstituteFor(product);
        //}
        internal bool isAbstractProduct()
        {
            return getProduct().isAbstract();
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
            //if (!this.isSameProductType(storage.getProduct()))
            if (!storage.isSameProductType(this.getProduct()))
            {
                Debug.Log("Storage subtract Outside failed - wrong product");
                set(0f);
            }
            else if (storage.isBiggerThan(this))
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Storage subtract Outside failed");
                set(0f);
            }
            else
                set(this.get() - storage.get());
            return this;
        }
        public void OnClicked()
        {
            if (!this.isAbstractProduct())
                MainCamera.tradeWindow.selectProduct((this).getProduct());
        }

        public Storage Copy()
        {
            return new Storage(this);
        }
    }
}