// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDns.Enum;
using Iot.Device.MulticastDns.Package;
using System.Collections;

namespace Iot.Device.MulticastDns.Entities
{
    /// <summary>
    /// The class whom represents a Multicast DNS message.
    /// </summary>
    public class Message
    {
        private static readonly System.Random _generator = new();

        private ushort _id;
        private ushort _flags = 0;

        /// <summary>
        /// The list of <see cref="Question">Questions</see> in the message.
        /// </summary>
        protected ArrayList _questions = new();
        /// <summary>
        /// The list of <see cref="Resource">Answers</see> in the message.
        /// </summary>
        protected ArrayList _answers = new();
        /// <summary>
        /// The list of <see cref="Resource">Servers</see> in the message.
        /// </summary>
        protected ArrayList _servers = new();
        /// <summary>
        /// The list of <see cref="Resource">Additional Resources</see> in the message.
        /// </summary>
        protected ArrayList _additionals = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class.
        /// </summary>
        public Message() : this(DnsHeaderFlags.Query)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class.
        /// </summary>
        /// <param name="flags">The header flags for this message.</param>
        public Message(DnsHeaderFlags flags)
        {
            _id = (ushort)_generator.Next(1 << 16);
            _flags = (ushort)flags;
        }

        /// <summary>
        /// Initializes an instance of Message.
        /// </summary>
        /// <param name="data">The byte[] containing a message.</param>
        public Message(byte[] data) => ParseData(data);

        /// <summary>
        /// Returns the <see cref="Question">Questions</see> in this message.
        /// </summary>
        /// <returns>An array of Questions.</returns>
        public Question[] GetQuestions() => (Question[])_questions.ToArray(typeof(Question));

        /// <summary>
        /// Returns the <see cref="Resource">Resources</see> in this message.
        /// </summary>
        /// <returns>An array of Resources.</returns>
        public Resource[] GetResources()
        {
            ArrayList resources = new();
            resources.AddRange(_answers);
            resources.AddRange(_servers);
            resources.AddRange(_additionals);
            return (Resource[])resources.ToArray(typeof(Resource));
        }

        /// <summary>
        /// Adds a Question to the message.
        /// </summary>
        /// <param name="question">The Question to add.</param>
        public void AddQuestion(Question question)
            => _questions.Add(question);

        /// <summary>
        /// Returns a byte[] representation of this message.
        /// </summary>
        /// <returns>A byte[] representation of this message.</returns>
        public byte[] GetBytes()
        {
            PacketBuilder packet = new();
            packet.Add(_id);
            packet.Add(_flags);
            packet.Add((ushort)_questions.Count);
            packet.Add((ushort)_answers.Count);
            packet.Add((ushort)_servers.Count);
            packet.Add((ushort)_additionals.Count);

            foreach (Question query in _questions)
            {
                packet.Add(query.GetBytes());
            }

            foreach (Resource resource in _answers)
            {
                packet.Add(resource.GetBytes());
            }

            foreach (Resource resource in _servers)
            {
                packet.Add(resource.GetBytes());
            }

            foreach (Resource resource in _additionals)
            {
                packet.Add(resource.GetBytes());
            }

            return packet.GetBytes();
        }

        private void ParseData(byte[] data)
        {
            PacketParser packet = new(data);

            _id = packet.ReadUShort();
            _flags = packet.ReadUShort();
            ushort question_count = packet.ReadUShort();
            ushort answer_count = packet.ReadUShort();
            ushort server_count = packet.ReadUShort();
            ushort additional_count = packet.ReadUShort();

            for (int i = 0; i < question_count; ++i)
            {
                string domain = packet.ReadDomain();
                DnsResourceType rrType = GetResourceType(packet.ReadUShort());
                ushort rrClass = packet.ReadUShort();
                if (rrType > 0) _questions.Add(new Question(domain, rrType, rrClass));
            }

            for (int i = 0; i < answer_count; ++i)
            {
                _answers.Add(ParseResource(packet));
            }

            for (int i = 0; i < server_count; ++i)
            {
                _servers.Add(ParseResource(packet));
            }

            for (int i = 0; i < additional_count; ++i)
            {
                _additionals.Add(ParseResource(packet));
            }
        }

        private Resource ParseResource(PacketParser packet)
        {
            string domain = packet.ReadDomain();
            ushort rrType = packet.ReadUShort();
            _ = packet.ReadUShort();
            int ttl = packet.ReadInt();
            ushort length = packet.ReadUShort();

            switch (GetResourceType(rrType))
            {
                case DnsResourceType.A: return new ARecord(packet, domain, ttl, length);
                case DnsResourceType.CNAME: return new CnameRecord(packet, domain, ttl);
                case DnsResourceType.PTR: return new PtrRecord(packet, domain, ttl);
                case DnsResourceType.TXT: return new TxtRecord(packet, domain, ttl);
                case DnsResourceType.AAAA: return new AaaaRecord(packet, domain, ttl, length);
                case DnsResourceType.SRV: return new SrvRecord(packet, domain, ttl);
                default:
                    packet.ReadBytes(length);
                    return new Resource(domain, ttl);
            }
        }

        private DnsResourceType GetResourceType(ushort rrType)
            => (DnsResourceType)rrType;
    }
}
