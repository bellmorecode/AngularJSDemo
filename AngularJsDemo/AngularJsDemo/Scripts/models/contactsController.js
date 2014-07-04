/// <reference path="../_references.js" />
// ng-app Module
var mainApp = angular.module('mainApp', ['contactsControllers','contactsServices'])

mainApp.filter('contactsFilter', function () {
    return function (contacts, query) {
        if (!query) {
            return contacts;
        }

        var filtered = [];
        query = query.toLowerCase();
        angular.forEach(contacts, function (person) {
            angular.forEach(person.Names, function (n) {
                //alert(n.Name)
                if (n.Name.toLowerCase().indexOf(query) !== -1) {
                    filtered.push(person);
                }
            });
        });

        return filtered;
    }
});

var contactsService = angular.module('contactsServices', ['ngResource']);
var contactsControllers = angular.module('contactsControllers', []);

contactsService.factory('Contacts', ['$resource',
    function ($resource) {
        return $resource('/Home/Contacts', {}, {
            query: { method: 'POST', params: {}, isArray: true }
        });
}]);

contactsControllers.controller('contactListController',
    ['$scope', 'Contacts',
        function ($scope, Contacts) {
            var contacts = [];
            Contacts.query(function (data) {
                angular.forEach(data, function (person) {
                    
                    contacts.push(person);
                });
                $scope.contacts = contacts;
            });
            
}]);
