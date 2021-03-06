﻿using DMSCommon;
using DMSCommon.Model;
using DMSCommon.TreeGraph;
using OMSSCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSService
{
    public class EnergizationAlgorithm
    {
        public static bool TraceUp(Node no, Tree<Element> tree)
        {
            //Element el = tree.Data[no.Parent];

            //if (tree.Data[el.ElementGID] is Source)
            //{
            //    return true;
            //}
            //else if (tree.Data[el.ElementGID] is Switch)
            //{
            //    Switch s = (Switch)tree.Data[el.ElementGID];
            //    if (s.Marker == true && s.State == SwitchState.Closed)
            //        return true;
            //    else
            //        return false;
            //}
            //else if (tree.Data[el.ElementGID] is ACLine)
            //{
            //    ACLine acl = (ACLine)tree.Data[el.ElementGID];
            //    Node n = (Node)tree.Data[acl.End1];

            //    if (TraceUp(n, tree))
            //        return true;
            //    else
            //        return false;
            //}
            //return false;

            while (true)
            {
                Element el = tree.Data[no.Parent];

                if (tree.Data[el.ElementGID] is Source)
                {
                    return true;
                }
                else if (tree.Data[el.ElementGID] is Switch)
                {
                    Switch s = (Switch)tree.Data[el.ElementGID];
                    if (s.Marker == true && s.State == SwitchState.Closed)
                        return true;
                    else
                        return false;
                }
                else if (tree.Data[el.ElementGID] is ACLine)
                {
                    ACLine acl = (ACLine)tree.Data[el.ElementGID];
                    Node n = (Node)tree.Data[acl.End1];

                    no = n;
                }
            }
        }

        public static List<UIUpdateModel> TraceDown(Node n, List<UIUpdateModel> networkChange, bool isEnergized, bool init, Tree<Element> tree)
        {
            //foreach (long item in n.Children)
            //{
            //    Element e = tree.Data[item];
            //    if (e is Consumer)
            //    {
            //        e.Marker = isEnergized;
            //        networkChange.Add(new SCADAUpdateModel(e.ElementGID, isEnergized));
            //    }
            //    else if (e is Switch)
            //    {
            //        Element switche;
            //        tree.Data.TryGetValue(e.ElementGID, out switche);
            //        Switch s = (Switch)switche;

            //        if (s.State == SwitchState.Open)
            //        {
            //            if (!init)
            //            {
            //                continue;
            //            }

            //            s.Marker = false;
            //            networkChange.Add(new SCADAUpdateModel(s.ElementGID, s.Marker, OMSSCADACommon.States.OPENED));
            //        }
            //        else
            //        {
            //            s.Marker = isEnergized;
            //            networkChange.Add(new SCADAUpdateModel(s.ElementGID, s.Marker, OMSSCADACommon.States.CLOSED));
            //        }

            //        Node node = (Node)tree.Data[s.End2];
            //        node.Marker = isEnergized;
            //        networkChange.Add(new SCADAUpdateModel(node.ElementGID, isEnergized));
            //        networkChange = TraceDown(node, networkChange, s.Marker, init, tree);
            //    }
            //    else if (e is ACLine)
            //    {
            //        Element acl;
            //        tree.Data.TryGetValue(e.ElementGID, out acl);
            //        ACLine ac = (ACLine)acl;
            //        ac.Marker = isEnergized;
            //        networkChange.Add(new SCADAUpdateModel(ac.ElementGID, isEnergized));
            //        Node node = (Node)tree.Data[ac.End2];
            //        node.Marker = isEnergized;
            //        networkChange.Add(new SCADAUpdateModel(node.ElementGID, isEnergized));
            //        networkChange = TraceDown(node, networkChange, isEnergized, init, tree);
            //    }
            //}

            Queue<Node> nodes = new Queue<Node>();
            nodes.Enqueue(n);

            while (nodes.Count > 0)
            {
                Node currentNode = nodes.Dequeue();

                foreach (long item in currentNode.Children)
                {
                    Element e = tree.Data[item];

                    if (e is Consumer)
                    {
                        e.Marker = currentNode.Marker;
                        networkChange.Add(new UIUpdateModel(e.ElementGID, isEnergized)); // mali bug koji se ne ispoljava, treba currentNode.Marker
                    }
                    else if (e is Switch)
                    {
                        Element switche;
                        tree.Data.TryGetValue(e.ElementGID, out switche);
                        Switch s = (Switch)switche;

                        if (s.State == SwitchState.Open)
                        {
                            if (!init)
                            {
                                continue;
                            }

                            s.Marker = false;
                            networkChange.Add(new UIUpdateModel(s.ElementGID, s.Marker, OMSSCADACommon.States.OPENED));
                        }
                        else
                        {
                            s.Marker = currentNode.Marker;
                            networkChange.Add(new UIUpdateModel(s.ElementGID, s.Marker, OMSSCADACommon.States.CLOSED));
                        }

                        Node node = (Node)tree.Data[s.End2];
                        node.Marker = s.Marker;
                        networkChange.Add(new UIUpdateModel(node.ElementGID, isEnergized));
                        nodes.Enqueue(node);
                    }
                    else if (e is ACLine)
                    {
                        Element acl;
                        tree.Data.TryGetValue(e.ElementGID, out acl);
                        ACLine ac = (ACLine)acl;
                        ac.Marker = currentNode.Marker;
                        networkChange.Add(new UIUpdateModel(ac.ElementGID, isEnergized));

                        Node node = (Node)tree.Data[ac.End2];
                        node.Marker = currentNode.Marker;
                        networkChange.Add(new UIUpdateModel(node.ElementGID, isEnergized));
                        nodes.Enqueue(node);
                    }
                }
            }

            return networkChange;
        }
    }
}
