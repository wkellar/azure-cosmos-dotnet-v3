﻿namespace Microsoft.Azure.Cosmos.CosmosElements.Patchable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Cosmos.Json;

    internal sealed class PatchableCosmosArray : PatchableCosmosElement
    {
        private readonly List<PatchableUnion> items;

        private PatchableCosmosArray(CosmosArray cosmosArray)
            : base(PatchableCosmosElementType.Array)
        {
            if (cosmosArray == null)
            {
                throw new ArgumentNullException($"{nameof(cosmosArray)}");
            }

            this.items = new List<PatchableUnion>();
            foreach (CosmosElement arrayItem in cosmosArray)
            {
                items.Add(PatchableUnion.Create(arrayItem));
            }
        }

        public PatchableCosmosElement this[int index]
        {
            get
            {
                return this.items[index];
            }
        }

        public int Count => this.items.Count;

        public static PatchableCosmosArray Create(CosmosArray cosmosArray)
        {
            return new PatchableCosmosArray(cosmosArray);
        }

        public void Add(int index, CosmosElement item)
        {
            this.items.Insert(index, PatchableUnion.Create(item));
        }

        public void Remove(int index)
        {
            this.items.RemoveAt(index);
        }

        public void Replace(int index, CosmosElement item)
        {
            this.items[index] = PatchableUnion.Create(item);
        }

        public override CosmosElement ToCosmosElement()
        {
            return new PatchableCosmosArrayWrapper(this);
        }

        private sealed class PatchableCosmosArrayWrapper : CosmosArray
        {
            private readonly PatchableCosmosArray patchableCosmosArray;
            public PatchableCosmosArrayWrapper(PatchableCosmosArray patchableCosmosArray)
            {
                if(patchableCosmosArray == null)
                {
                    throw new ArgumentNullException($"{nameof(patchableCosmosArray)}");
                }

                this.patchableCosmosArray = patchableCosmosArray;
            }

            public override CosmosElement this[int index]
            {
                get
                {
                    PatchableUnion patchableUnion = this.patchableCosmosArray.items[index];
                    return patchableUnion.CosmosElement;
                }
            }

            public override int Count => this.patchableCosmosArray.items.Count;

            public override IEnumerator<CosmosElement> GetEnumerator()
            {
                return this.patchableCosmosArray
                    .items
                    .Select((patchableUnion) => patchableUnion.CosmosElement)
                    .GetEnumerator();
            }

            public override void WriteTo(IJsonWriter jsonWriter)
            {
                jsonWriter.WriteArrayStart();

                foreach(CosmosElement cosmosElement in this)
                {
                    cosmosElement.WriteTo(jsonWriter);
                }

                jsonWriter.WriteArrayEnd();
            }
        }
    }
}