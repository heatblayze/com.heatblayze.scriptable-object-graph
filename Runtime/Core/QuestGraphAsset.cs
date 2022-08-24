using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace ScriptableObjectGraph.Core
{
    [CreateAssetMenu(menuName = "RPG Tools/Quest Graph", order = 120)]
    public class QuestGraphAsset : GuidScriptable, INodeContainer<Questline>
    {
        public List<Questline> Questlines => _questlines;
        [SerializeField]
        List<Questline> _questlines = new List<Questline>();

        public QuestAttributeList Attributes => _attributes;
        [SerializeField]
        QuestAttributeList _attributes;

        public Questline EntryNode => _entryNode;

        public string EditorWindowPrefix => "Questline";

        [SerializeField]
        Questline _entryNode;

        public IEnumerable<Questline> GetNodes()
        {
            return _questlines;
        }

        public NodeBase CreateNode(Type type)
        {
#if UNITY_EDITOR
            return CreateQuestline(type);
#else
            return null;
#endif
        }

        #region Editor
#if UNITY_EDITOR        
        public Questline CreateQuestline(System.Type type)
        {
            Questline questline = CreateInstance(type) as Questline;
            questline.name = type.Name;
            questline.guid = System.Guid.NewGuid().ToString();

            if (_entryNode == null)
                _entryNode = questline;
            _questlines.Add(questline);

            AssetDatabase.AddObjectToAsset(questline, this);
            AssetDatabase.SaveAssets();
            return questline;
        }

        public void DeleteQuestline(Questline questline)
        {
            if(questline == null) return;
            if (!_questlines.Contains(questline)) return;

            if (_entryNode == questline)
                _entryNode = null;

            _questlines.Remove(questline);

            AssetDatabase.RemoveObjectFromAsset(questline);
            AssetDatabase.SaveAssets();
        }
#endif
        #endregion
    }
}
