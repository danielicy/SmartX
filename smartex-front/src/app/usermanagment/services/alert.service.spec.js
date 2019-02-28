"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var testing_1 = require("@angular/core/testing");
var alert_service_1 = require("./alert.service");
describe('AlertService', function () {
    beforeEach(function () { return testing_1.TestBed.configureTestingModule({}); });
    it('should be created', function () {
        var service = testing_1.TestBed.get(alert_service_1.AlertService);
        expect(service).toBeTruthy();
    });
});
//# sourceMappingURL=alert.service.spec.js.map