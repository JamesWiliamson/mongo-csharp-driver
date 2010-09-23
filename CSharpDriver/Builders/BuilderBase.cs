﻿/* Copyright 2010 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using MongoDB.BsonLibrary;
using MongoDB.BsonLibrary.IO;

namespace MongoDB.CSharpDriver.Builders {
    public abstract class BuilderBase {
        #region constructors
        protected BuilderBase() {
        }
        #endregion

        #region public methods
        public string ToJson() {
            return ToJson(BsonJsonWriterSettings.Defaults);
        }

        public string ToJson(
            BsonJsonWriterSettings settings
        ) {
            StringWriter stringWriter = new StringWriter();
            using (BsonWriter bsonWriter = BsonWriter.Create(stringWriter, settings)) {
                BsonSerializer serializer = new BsonSerializer();
                serializer.Serialize(bsonWriter, this, false); // don't serializeIdFirst
            }
            return stringWriter.ToString();
        }

        public override string ToString() {
            return ToJson();
        }
        #endregion
    }
}
