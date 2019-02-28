"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var testing_1 = require("@angular/core/testing");
var tweet_service_1 = require("./tweet.service");
describe('TweetService', function () {
    beforeEach(function () { return testing_1.TestBed.configureTestingModule({}); });
    it('should be created', function () {
        var service = testing_1.TestBed.get(tweet_service_1.TweetService);
        expect(service).toBeTruthy();
    });
});
//# sourceMappingURL=tweet.service.spec.js.map