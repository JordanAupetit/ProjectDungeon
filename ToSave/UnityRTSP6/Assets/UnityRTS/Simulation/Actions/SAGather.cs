using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation {
    public class SAGather : SAInteract {

        public int Capacity = 10;
        public int GatherAmount = 1;
        public float DropRange = 1;
        public float DropTime = 0.5f;

        public int HoldingAmount { get; private set; }
        public string HoldingType { get; private set; }

        public override float AugmentedRange { get { return SubRequest.TargetComponent is SCResourceDropSite ? DropRange : base.AugmentedRange; } }
        public override float AugmentedInterval { get { return SubRequest.TargetComponent is SCResourceDropSite ? DropTime : base.AugmentedInterval; } }

        public override float ScoreRequest(ActionRequest request) {
            return
                request.RequireComponent<SCResourceSite>() != null ? 2 :
                request.RequireComponent<SCResourceDropSite>() != null ? 1.5f :
                0;
        }

        public override void Begin(ActionRequest request) {
            // Dont require the same type if we were re-ordered
            // and are not holding any resources
            if (HoldingAmount == 0) ClearHolding();
            // Drop any resources we have if we're targeted
            // at a different resource
            if (HoldingType != null) {
                var site = request.RequireComponent<SCResourceSite>();
                if (site != null && site.Resources.GetResource(HoldingType) <= 0) ClearHolding();
            }
            base.Begin(request);
        }

        protected override void NotifyInterval(ActionRequest request) {
            if (request.TargetComponent is SCResourceSite && HoldingType != null) {
                // Gathering resources
                var site = request.RequireComponent<SCResourceSite>();
                if (site != null) HoldingAmount += site.TakeResources(HoldingType, Mathf.Min(GatherAmount, Capacity - HoldingAmount));
                if (HoldingAmount >= Capacity || (site == null || !site.Resources.HasResources)) EndStage();
            } else if (request.TargetComponent is SCResourceDropSite && HoldingType != null) {
                // Delivering resources
                var site = request.RequireComponent<SCResourceDropSite>();
                if (site != null) HoldingAmount -= site.DeliverResources(HoldingType, HoldingAmount);
                EndStage();
            } else {
                Debug.LogError("Invalid target for Gathering: " + request);
                EndStage();
            }
            base.NotifyInterval(request);
            RequireStage();
        }

        protected override void BeginStage() {
            if (HoldingAmount < Capacity) {
                var site = Request.RequireComponent<SCResourceSite>();
                if (site == null) site = Entity.FindNearby<SCResourceSite>(e => e.Resources.HasResources);
                BeginStage(new ActionRequest(site));
            } else {
                var site = Request.RequireComponent<SCResourceDropSite>();
                if (site == null) site = Entity.FindNearby<SCResourceDropSite>(e => e.Entity.Player == Entity.Player);
                BeginStage(new ActionRequest(site));
            }
        }
        protected override void BeginStage(ActionRequest request) {
            Debug.Log("Starting gathering " + request);
            if (HoldingType == null) {
                // TODO: Fix this, have it choose a valid resource type
                // either that can be gathered, or can be delivered
                if (request.TargetComponent is SCResourceSite) {
                    HoldingType = request.RequireComponent<SCResourceSite>().Resources.Resources[0].Name;
                } else if (request.TargetComponent is SCResourceDropSite) {
                    //holdingType = ((ResourceDropSite)component).Resources.Resources[0].Name;
                }
            }
            base.BeginStage(request);
        }

        public void ClearHolding() {
            HoldingType = null;
            HoldingAmount = 0;
        }

        public override void CloneFrom(SimComponent component) {
            var other = (SAGather)component;
            Capacity = other.Capacity;
            GatherAmount = other.GatherAmount;
            DropRange = other.DropRange;
            DropTime = other.DropTime;
            HoldingAmount = other.HoldingAmount;
            HoldingType = other.HoldingType;
            base.CloneFrom(component);
        }
    }
}
