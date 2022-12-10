using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using OWML.Common;
using System.IO;
using System.Xml;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.Components;

namespace NewHorizons.Builder.Props
{
    public static class DialogueBuilder
    {
        // Returns the character dialogue tree and remote dialogue trigger, if applicable.
        public static (CharacterDialogueTree, RemoteDialogueTrigger) Make(GameObject go, Sector sector, PropModule.DialogueInfo info, IModBehaviour mod)
        {
            // In stock I think they disable dialogue stuff with conditions
            // Here we just don't make it at all
            if (info.blockAfterPersistentCondition != null && PlayerData.GetPersistentCondition(info.blockAfterPersistentCondition)) return (null, null);

            var dialogue = MakeConversationZone(go, sector, info, mod.ModHelper);
            
            RemoteDialogueTrigger remoteTrigger = null;
            if (info.remoteTriggerPosition != null || info.remoteTriggerRadius != 0)
            {
                remoteTrigger = MakeRemoteDialogueTrigger(go, sector, info, dialogue);
            }

            if (!string.IsNullOrEmpty(info.rename))
            {
                dialogue.name = info.rename;
                if (remoteTrigger != null)
                {
                    remoteTrigger.name = $"{info.rename}_{remoteTrigger.name}";
                }
            }

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var parent = go.transform.Find(info.parentPath);
                if (parent != null)
                {
                    dialogue.transform.parent = parent;
                    if (remoteTrigger != null)
                    {
                        remoteTrigger.transform.parent = parent;
                    }
                }
            }

            // Make the character look at the player
            // Useful for dialogue replacement
            // Overrides parent path for dialogue
            if (!string.IsNullOrEmpty(info.pathToAnimController))
            {
                MakePlayerTrackingZone(go, dialogue, info);
            }

            return (dialogue, remoteTrigger);
        }

        private static RemoteDialogueTrigger MakeRemoteDialogueTrigger(GameObject planetGO, Sector sector, PropModule.DialogueInfo info, CharacterDialogueTree dialogue)
        {
            var conversationTrigger = new GameObject("ConversationTrigger");
            conversationTrigger.SetActive(false);

            var remoteDialogueTrigger = conversationTrigger.AddComponent<RemoteDialogueTrigger>();
            var sphereCollider = conversationTrigger.AddComponent<SphereCollider>();
            conversationTrigger.AddComponent<OWCollider>();

            remoteDialogueTrigger._listDialogues = new RemoteDialogueTrigger.RemoteDialogueCondition[]
            {
                new RemoteDialogueTrigger.RemoteDialogueCondition()
                {
                    priority = 1,
                    dialogue = dialogue,
                    prereqConditionType = RemoteDialogueTrigger.MultiConditionType.AND,
                    prereqConditions = new string[]{ },
                    onTriggerEnterConditions = new string[]{ }
                }
            };
            remoteDialogueTrigger._activatedDialogues = new bool[1];
            remoteDialogueTrigger._deactivateTriggerPostConversation = true;

            sphereCollider.radius = info.remoteTriggerRadius == 0 ? info.radius : info.remoteTriggerRadius;

            conversationTrigger.transform.parent = sector?.transform ?? planetGO.transform;
            conversationTrigger.transform.position = planetGO.transform.TransformPoint(info.remoteTriggerPosition ?? info.position);
            conversationTrigger.SetActive(true);

            return remoteDialogueTrigger;
        }

        private static CharacterDialogueTree MakeConversationZone(GameObject planetGO, Sector sector, PropModule.DialogueInfo info, IModHelper mod)
        {
            var conversationZone = new GameObject("ConversationZone");
            conversationZone.SetActive(false);

            conversationZone.layer = LayerMask.NameToLayer("Interactible");

            var sphere = conversationZone.AddComponent<SphereCollider>();
            sphere.radius = info.radius;
            sphere.isTrigger = true;

            var owCollider = conversationZone.AddComponent<OWCollider>();
            var interact = conversationZone.AddComponent<InteractReceiver>();

            interact._interactRange = info.range;

            if (info.radius <= 0)
            {
                sphere.enabled = false;
                owCollider.enabled = false;
                interact.enabled = false;
            }

            var dialogueTree = conversationZone.AddComponent<NHCharacterDialogueTree>();

            var xml = File.ReadAllText(Path.Combine(mod.Manifest.ModFolderPath, info.xmlFile));
            var text = new TextAsset(xml)
            {
                // Text assets need a name to be used with VoiceMod
                name = Path.GetFileNameWithoutExtension(info.xmlFile)
            };

            dialogueTree.SetTextXml(text);
            AddTranslation(xml);

            conversationZone.transform.parent = sector?.transform ?? planetGO.transform;
            
            if (!string.IsNullOrEmpty(info.parentPath))
            {
                conversationZone.transform.parent = planetGO.transform.Find(info.parentPath);
            }
            else if (!string.IsNullOrEmpty(info.pathToAnimController))
            {
                conversationZone.transform.parent = planetGO.transform.Find(info.pathToAnimController);
            }

            var pos = (Vector3)(info.position ?? Vector3.zero);
            if (info.isRelativeToParent) conversationZone.transform.localPosition = pos;
            else conversationZone.transform.position = planetGO.transform.TransformPoint(pos);

            conversationZone.SetActive(true);

            return dialogueTree;
        }

        private static void MakePlayerTrackingZone(GameObject go, CharacterDialogueTree dialogue, PropModule.DialogueInfo info)
        {
            var character = go.transform.Find(info.pathToAnimController);

            if (character == null)
            {
                Logger.LogError($"Couldn't find child of {go.transform.GetPath()} at {info.pathToAnimController}");
                return;
            }

            // At most one of these should ever not be null
            var nomaiController = character.GetComponent<SolanumAnimController>();
            var controller = character.GetComponent<CharacterAnimController>();

            var lookOnlyWhenTalking = info.lookAtRadius <= 0;

            // To have them look when you start talking
            if (controller != null)
            {
                controller._dialogueTree = dialogue;
                controller.lookOnlyWhenTalking = lookOnlyWhenTalking;
            }
            else if (nomaiController != null)
            {
                if (lookOnlyWhenTalking)
                {
                    dialogue.OnStartConversation += nomaiController.StartWatchingPlayer;
                    dialogue.OnEndConversation += nomaiController.StopWatchingPlayer;
                }
            }
            else
            {
                // TODO: make a custom controller for basic characters to just turn them to face you
            }

            if (info.lookAtRadius > 0)
            {
                var playerTrackingZone = new GameObject("PlayerTrackingZone");
                playerTrackingZone.SetActive(false);

                playerTrackingZone.layer = LayerMask.NameToLayer("BasicEffectVolume");
                playerTrackingZone.SetActive(false);

                var sphereCollider = playerTrackingZone.AddComponent<SphereCollider>();
                sphereCollider.radius = info.lookAtRadius;
                sphereCollider.isTrigger = true;

                playerTrackingZone.AddComponent<OWCollider>();

                var triggerVolume = playerTrackingZone.AddComponent<OWTriggerVolume>();

                if (controller)
                {
                    // Since the Awake method is CharacterAnimController was already called 
                    if (controller.playerTrackingZone)
                    {
                        controller.playerTrackingZone.OnEntry -= controller.OnZoneEntry;
                        controller.playerTrackingZone.OnExit -= controller.OnZoneExit;
                    }
                    // Set it to use the new zone
                    controller.playerTrackingZone = triggerVolume;
                    triggerVolume.OnEntry += controller.OnZoneEntry;
                    triggerVolume.OnExit += controller.OnZoneExit;
                }
                // Simpler for the Nomai
                else if (nomaiController)
                {
                    triggerVolume.OnEntry += (_) => nomaiController.StartWatchingPlayer();
                    triggerVolume.OnExit += (_) => nomaiController.StopWatchingPlayer();
                }
                // No controller
                else
                {
                    // TODO
                }

                playerTrackingZone.transform.parent = dialogue.gameObject.transform;
                playerTrackingZone.transform.localPosition = Vector3.zero;

                playerTrackingZone.SetActive(true);
            }
        }

        private static void AddTranslation(string xml)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var xmlNode = xmlDocument.SelectSingleNode("DialogueTree");
            var xmlNodeList = xmlNode.SelectNodes("DialogueNode");
            string characterName = xmlNode.SelectSingleNode("NameField").InnerText;
            TranslationHandler.AddDialogue(characterName);

            foreach (object obj in xmlNodeList)
            {
                var xmlNode2 = (XmlNode)obj;
                var name = xmlNode2.SelectSingleNode("Name").InnerText;

                var xmlText = xmlNode2.SelectNodes("Dialogue/Page");
                foreach (object page in xmlText)
                {
                    var pageData = (XmlNode)page;
                    var text = pageData.InnerText;
                    // The text is trimmed in DialogueText constructor (_listTextBlocks), so we also need to trim it for the key
                    TranslationHandler.AddDialogue(text, true, name);
                }

                xmlText = xmlNode2.SelectNodes("DialogueOptionsList/DialogueOption/Text");
                foreach (object option in xmlText)
                {
                    var optionData = (XmlNode)option;
                    var text = optionData.InnerText;
                    // The text is trimmed in CharacterDialogueTree.LoadXml, so we also need to trim it for the key
                    TranslationHandler.AddDialogue(text, true, characterName, name);
                }
            }
        }
    }
}
