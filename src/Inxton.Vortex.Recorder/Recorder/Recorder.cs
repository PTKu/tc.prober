﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Vortex.Connector;


namespace Inxton.Vortex.Recorder
{
    internal class RecorderBase<T, P> where T : IVortexObject, new() where P : new()
    {
        public RecorderBase(T obj)
        {
            Object = obj;
        }

        protected T Object { get; }

        protected static Recording<P> LoadRecording(string fileName)
        {
            var rec = new Recording<P>();
            using (StreamReader sr = new StreamReader(fileName))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    var serializer = new Newtonsoft.Json.JsonSerializer();
                    rec = serializer.Deserialize<Recording<P>>(reader);
                }
            }

            return rec;
        }

        protected static Recording<P> Squash(Recording<P> recording)
        {
            var squashed = new Recording<P>();

            var adapter = new ConnectorAdapter(typeof(DummyConnectorFactory));
            var connector = adapter.GetConnector(new object[] { });

            T current = (T)Activator.CreateInstance(typeof(T), connector, string.Empty, string.Empty);
            T next = (T)Activator.CreateInstance(typeof(T), connector, string.Empty, string.Empty);

            for (int i = 0; i < recording.Frames.LongCount(); i++)
            {
                var currentFrame = recording.Frames[i];
                P currentObject = currentFrame.Object;
                long currentObjectStamp = recording.Frames[i].Stamp;
                CopyToOnline(current, currentObject);
                var currentTags = current.RetrieveValueTags().ToArray();


                var nextFrame = currentFrame;
                P nextObject = currentObject;
                long nextObjectStamp = currentObjectStamp;
                CopyToOnline(next, nextObject);
                var nextTags = next.RetrieveValueTags().ToArray();

                if (i < recording.Frames.LongCount() - 1)
                {
                    nextFrame = recording.Frames[i + 1];
                    nextObject = nextFrame.Object;
                    nextObjectStamp = nextFrame.Stamp;
                    CopyToOnline(next, nextObject);
                    nextTags = next.RetrieveValueTags().ToArray();
                }


                var isSame = true;

                for (int next_index = 0; next_index < nextTags.LongCount(); next_index++)
                {
                    if (((dynamic)nextTags[next_index]).Cyclic != ((dynamic)currentTags[next_index]).Cyclic)
                    {
                        isSame = false;
                        break;
                    }
                }

                if (squashed.Frames.LongCount() == 0)
                {
                    squashed.Frames.Add(currentFrame);
                }

                if (isSame)
                {
                    squashed.Frames.Last().Stamp = nextObjectStamp;
                }
                else
                {
                    squashed.Frames.Add(nextFrame);
                }
            }

            return squashed;
        }

        protected static object GetPlainerCopyNow(dynamic obj)
        {
            var plain = obj.CreatePlainerType();
            obj.FlushOnlineToPlain(plain);
            return plain;
        }

        protected static void CopyToOnline(dynamic obj, P source)
        {
            obj.FlushPlainToOnline(source);
        }

        public Recording<P> Recording
        {
            get { return this.recording; }
        }

        protected Recording<P> recording
        {
            get;
            set;
        } = new Recording<P>();        
    }
    internal class Dummy : IRecorder
    {
        public void Act()
        {
            
        }

        public void Begin(string fileName)
        {
            
        }

        public void End(string fileName)
        {
            
        }
    }

    public interface IRecorder
    {
        void Begin(string fileName);
        void Act();
        void End(string fileName);
    }


    public enum RecorderModeEnum
    {
        None,
        Player,
        Graver
    }


    public class Recorder<T,P> where T : IVortexObject, new() where P : new()
    {     
        public Recorder(T obj, RecorderModeEnum mode, long minUniqueFrames = 10)
        {
            this.Mode = mode;

            switch (mode)
            {
                case RecorderModeEnum.None:
                    Actor = new Dummy();
                    break;
                case RecorderModeEnum.Player:
                    Actor = new Player<T, P>(obj);
                    break;
                case RecorderModeEnum.Graver:
                    Actor = new Graver<T, P>(obj, minUniqueFrames);
                    break;          
            }                        
        }

        public RecorderModeEnum Mode
        {
            get;           
        }
            
        public IRecorder Actor
        {
            get;            
        }
    }
}
