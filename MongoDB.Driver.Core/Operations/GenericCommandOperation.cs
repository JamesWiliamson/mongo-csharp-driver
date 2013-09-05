﻿/* Copyright 2010-2013 10gen Inc.
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Diagnostics;
using MongoDB.Driver.Core.Protocol.Messages;
using MongoDB.Driver.Core.Sessions;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a generic command.
    /// </summary>
    /// <typeparam name="TCommandResult">The type of the command result.</typeparam>
    public sealed class GenericCommandOperation<TCommandResult> : CommandOperationBase<TCommandResult> where TCommandResult : CommandResult
    {
        // private fields
        private BsonDocument _command;
        private DatabaseNamespace _database;
        private bool _isQuery;
        private ReadPreference _readPreference;
        private IBsonSerializer<TCommandResult> _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCommandOperation{TCommandResult}" /> class.
        /// </summary>
        public GenericCommandOperation()
        {
            _readPreference = ReadPreference.Primary;
        }

        // public properties
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public BsonDocument Command
        {
            get { return _command; }
            set { _command = value; }
        }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        public DatabaseNamespace Database
        {
            get { return _database; }
            set { _database = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is query.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is query; otherwise, <c>false</c>.
        /// </value>
        public bool IsQuery
        {
            get { return _isQuery; }
            set { _isQuery = value; }
        }

        /// <summary>
        /// Gets or sets the read preference.
        /// </summary>
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
            set { _readPreference = value; }
        }

        /// <summary>
        /// Gets or sets the serializer.
        /// </summary>
        public IBsonSerializer<TCommandResult> Serializer
        {
            get { return _serializer; }
            set { _serializer = value; }
        }

        // public methods
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>The command result.</returns>
        public override TCommandResult Execute()
        {
            EnsureRequiredProperties();

            using (__trace.TraceActivity("GenericCommandOperation"))
            using (var channelProvider = CreateServerChannelProvider(new ReadPreferenceServerSelector(_readPreference), _isQuery))
            {
                if (__trace.Switch.ShouldTrace(TraceEventType.Verbose))
                {
                    __trace.TraceVerbose("running {0} on database {1} at {2}.", _command.ToJson(), _database.DatabaseName, channelProvider.Server.DnsEndPoint);
                }

                var args = new ExecuteCommandProtocolArgs<TCommandResult>
                {
                    Command = _command,
                    CommandResultSerializer = _serializer,
                    Database = _database,
                    ReadPreference = _readPreference
                };

                return ExecuteCommandProtocol(channelProvider, args);
            }
        }

        // protected methods
        /// <summary>
        /// Ensures that required properties have been set or provides intelligent defaults.
        /// </summary>
        protected override void EnsureRequiredProperties()
        {
            base.EnsureRequiredProperties();
            Ensure.IsNotNull("Command", _command);
            Ensure.IsNotNull("Database", _database);
            Ensure.IsNotNull("ReadPreference", _readPreference);
            if (_serializer == null)
            {
                _serializer = BsonSerializer.LookupSerializer<TCommandResult>();
            }
        }
    }
}