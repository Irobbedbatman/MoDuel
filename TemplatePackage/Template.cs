﻿using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.State;

namespace TemplatePackage;
public class TemplatePackage : PackagedCode {

    public TemplatePackage(Package package) : base(package) { }

    public override ICollection<Delegate> GetAllActions() {
        return GetAllActionsViaTag();
    }

    public override void OnDuelLoaded(DuelState state) {
        throw new NotImplementedException();
    }

    public override void OnPackageLoaded(Package package) {
        throw new NotImplementedException();
    }
}