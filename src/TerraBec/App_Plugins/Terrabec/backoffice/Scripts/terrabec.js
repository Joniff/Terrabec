angular.module('umbraco').controller('terrabec.connector',
	[
		'$scope', '$route', '$routeParams', '$q', '$timeout', 'navigationService', 'localizationService', 'terrabecService',
		function ($scope, $route, $routeParams, $q, $timeout, navigationService, localizationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				data: {},
				loading: true,
				nodeId: $routeParams.id,
				overview: null,
				info: null,
				refreshButtonText: '',
				init: function () {
					$q.all({
						overview: terrabecService.connector(vm.nodeId),
						info: terrabecService.connectorInfo(vm.nodeId),
						translateRefresh: localizationService.localize('TerrabecInfo_RefreshButton')
					}).then(function (response) {
						vm.overview = response.overview;
						vm.info = response.info;
						vm.refreshButtonText = response.translateRefresh;
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1, $routeParams.id], forceReload: false });
							vm.loading = false;
						});
					});
				},
				refresh: function () {
					$q.all({
						refresh: terrabecService.refreshConnector(vm.nodeId)
					}).then(function () {
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1], forceReload: true }).then(function () {
								$route.reload();
							});
							//navigationService.reloadNode($scope.currentNode.parent());
						});
					});
				}

			});
		}
	]
);

angular.module('umbraco.directives').directive('terrabecIframe',
	['$compile',
	function ($compile) {
		return {
			restrict: 'E',
			template: '<iframe></iframe>',
			scope: {
				iframeTemplate: '=',
				iframeContext: '='
			},
			link: function ($scope, $element, $attrs) {

				var setScope = function () {
					angular.forEach($scope.iframeContext, function (value, key) {
						$scope[key] = value;
					});
					setContextWatchers();
				};

				var render = function () {
					$compile($element
						.find('iframe').contents()
						.find('body').html($scope.iframeTemplate)
						.contents()
					)($scope);
				};

				var setContextWatchers = function () {
					angular.forEach($scope.iframeContext, function (value, key) {
						$scope.$watch(
							function ($scope) {
								return $scope.iframeContext[key];
							},
							function () {
								$scope[key] = $scope.iframeContext[key];
							});
					});
				};

				$scope.$watch('iframeTemplate', () => {
					setScope();
					render('template');
				});
			}
		}
	}
]);
angular.module('umbraco.directives').directive('terrabecDynamicHtml',
	['$compile',
	function ($compile) {
		return {
			restrict: 'E',
			scope: {
				contents: '='
			},
			link: function ($scope, $element, $attrs) {
				let element = $element;
				$scope.$watch('contents', function () {
					let newElement = $compile($scope.contents)($scope);
					element.replaceWith(newElement);
					element = newElement;
				});
			}
		};
	}
]);

angular.module('umbraco').controller('terrabec.email',
	[
		'$scope', '$routeParams', '$location', '$q', '$timeout', 'navigationService', 'terrabecService',
		function ($scope, $routeParams, $location, $q, $timeout, navigationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				loading: true,
				nodeId: $routeParams.id,
				emails: [],
				email: null,
				overview: null,
				info: null,
				emailAddress: null,
				path: [],
				iframe: {},
				htmlDecode: function (input)
				{
					var doc = new DOMParser().parseFromString(input, 'text/html');
					return doc.documentElement.textContent;
				},
				init: function (all) {
					$q.all({
						overview: terrabecService.connector(vm.nodeId),
						emails: all ? terrabecService.enumerateEmailTemplates(vm.nodeId) : terrabecService.readEmailTemplate(vm.nodeId)
					}).then(function (response) {
						vm.overview = response.overview;
						vm.path = [-1, vm.overview.nodeId];
						if (all) {
							vm.emails = response.emails;
						} else {
							vm.path.push(vm.overview.emailTemplatesNodeId);
							vm.email = response.emails;
							vm.iframe = JSON.parse('"' + vm.email.content + '"');
						}
						vm.path.push($routeParams.id);
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: vm.path, forceReload: false });
							vm.loading = false;
						});

					});
				},
				selectEmailTemplate: function (nodeId) {
					$location.path('/' + $routeParams.section + '/' + $routeParams.section + '/emailtemplate/' + nodeId);
				}
			});
		}
	]
);

angular.module('umbraco').controller('terrabec.list',
	[
		'$scope', '$route', '$routeParams', '$location', '$q', '$timeout', 'notificationsService', 'treeService', 'navigationService', 'terrabecService',
		function ($scope, $route, $routeParams, $location, $q, $timeout, notificationsService, treeService, navigationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				loading: 0,
				edit: 1,
				saving: 2,
				saved: 3,
				error: 4,
				state: 0,
				nodeId: $routeParams.id,
				lists: [],
				list: null,
				overview: null,
				info: null,
				subscriptions: [],
				listName: null,
				path: [],
				init: function (all) {
					$q.all({
						overview: terrabecService.connector(vm.nodeId),
						lists: (all) ? terrabecService.enumerateLists(vm.nodeId) : terrabecService.readList(vm.nodeId),
					}).then(function (response) {
						vm.overview = response.overview;
						vm.path = [-1, vm.overview.nodeId];
						if (all) {
							vm.lists = response.lists;
						} else {
							vm.list = response.lists;
							vm.path.push(vm.overview.listsNodeId);
						}
						vm.path.push($routeParams.id);
						$q.all({
							subscriptions: terrabecService.readSubscriptions(vm.nodeId)
						}).then(function (response) {
							vm.subscriptions = response.subscriptions;
							$timeout(function () {
								navigationService.syncTree({ tree: $routeParams.section, path: vm.path, forceReload: false });
								vm.state = vm.edit;
							});

							//$scope.$watch('createList.listName', function (newValue, oldValue) {
							//	if (newValue) {
							//		for (var i = 0; i != vm.lists.length; i++) {
							//			if (vm.lists[i].name == newValue) {
							//				$scope.createList.listName.$setValidity('unqiue', false);
							//				return;
							//			}
							//		}
							//		$scope.createList.listName.$setValidity('unqiue', true);
							//	}
							//});
						});

					});
				},
				selectList: function (nodeId) {
					$location.path('/' + $routeParams.section + '/' + $routeParams.section + '/list/' + nodeId);
				},
				createList: function (name) {
					vm.state = vm.saving;
					$q.all({
						list: terrabecService.createList(vm.nodeId, name)
					}).then(function (response) {
						vm.list = response.list;
						vm.state = vm.saved;
					}, function () {
						vm.state = vm.error;
					});
				},
				close: function () {
					navigationService.hideDialog();
					$timeout(function () {
						navigationService.reloadNode($scope.currentNode.parent());
						$route.reload();
					});
				}
			});
		}
	]
);

angular.module('umbraco').controller('terrabec.overview',
	[
		'$scope', '$route', '$routeParams', '$location', '$q', '$timeout','navigationService', 'terrabecService',
		function ($scope, $route, $routeParams, $location, $q, $timeout, navigationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				loading: true,
				refreshing: true,
				connectors: null,
				subscriptions: null,
				init: function () {
					$q.all({
						connectors: terrabecService.connectors(),
						subscriptions: terrabecService.readSubscriptions(),
					}).then(function (response) {
						vm.connectors = response.connectors;
						vm.subscriptions = response.subscriptions;
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1], forceReload: false });
							vm.loading = false;
						});
					});
				},
				selectInfo: function (nodeId) {
					$location.path('/' + $routeParams.section + '/' + $routeParams.section + '/Connector/' + nodeId);
				},
				refresh: function () {
					$q.all({
						refresh: terrabecService.refresh()
					}).then(function () {
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1], forceReload: true }).then(function () {
								$route.reload();
							});
							//navigationService.reloadNode($scope.currentNode.parent());
						});
					});
				},
				close: function () {
					navigationService.hideDialog();
				}
			});
		}
	]
);

angular.module('umbraco').controller('terrabec.queue',
	[
		'$scope', '$route', '$routeParams', '$q', '$timeout', 'navigationService', 'localizationService', 'terrabecService',
		function ($scope, $route, $routeParams, $q, $timeout, navigationService, localizationService, terrabecService) {
			var vm = this;

			angular.extend(vm, {
				data: {},
				loading: true,
				nodeId: $routeParams.id,
				overview: null,
				info: null,
				refreshButtonText: '',
				init: function () {
					$q.all({
						overview: terrabecService.queue(vm.nodeId),
						info: terrabecService.queueInfo(vm.nodeId),
						translateRefresh: localizationService.localize('TerrabecInfo_RefreshButton')
					}).then(function (response) {
						vm.overview = response.overview;
						vm.info = response.info;
						vm.refreshButtonText = response.translateRefresh;
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1, $routeParams.id], forceReload: false });
							vm.loading = false;
						});
					});
				},
				refresh: function () {
					$q.all({
						refresh: terrabecService.refreshQueue(vm.nodeId)
					}).then(function () {
						$timeout(function () {
							navigationService.syncTree({ tree: $routeParams.section, path: [-1], forceReload: true }).then(function () {
								$route.reload();
							});
							//navigationService.reloadNode($scope.currentNode.parent());
						});
					});
				}

			});
		}
	]
);

angular.module('umbraco.resources').factory('terrabecService', ['$http', '$q', function ($http, $q) {

	var apiRoot = '/umbraco/backoffice/terrabec/TerrabecApi/';

	function get(url) {
		var deferred = $q.defer();
		$http.get(url).then(function (response) {
			return deferred.resolve(response.data);
		}, function (response) {
			return deferred.reject(response);
		});
		return deferred.promise;
	}

	return {
		refreshConnector: function (id) {
			return get(apiRoot + 'refreshConnector?nodeId=' + id);
		},
		refreshQueue: function (id) {
			return get(apiRoot + 'refreshQueue?nodeId=' + id);
		},
		connectors: function () {
			return get(apiRoot + 'connectors');
		},
		connector: function (id) {
			return get(apiRoot + 'connector?nodeId=' + id);
		},
		connectorInfo: function (id) {
			return get(apiRoot + 'connectorInfo?nodeId=' + id);
		},
		enumerateLists: function (id) {
			return get(apiRoot + 'connectorEnumerateLists?nodeId=' + id);
		},
		readList: function (id) {
			return get(apiRoot + 'ConnectorReadList?nodeId=' + id);
		},
		createList: function (id, name) {
			return get(apiRoot + 'ConnectorCreateList?nodeId=' + id + "&name=" + name);
		},
		queues: function () {
			return get(apiRoot + 'queues');
		},
		queue: function (id) {
			return get(apiRoot + 'queue?nodeId=' + id);
		},
		queueInfo: function (id) {
			return get(apiRoot + 'queueInfo?nodeId=' + id);
		},
		enumerateEmailTemplates: function (id) {
			return get(apiRoot + 'ConnectorEnumerateEmailTemplates?nodeId=' + id);
		},
		readEmailTemplate: function (id) {
			return get(apiRoot + 'ConnectorReadEmailTemplate?nodeId=' + id);
		},
		readSubscriptions: function (id) {
			if (typeof id === 'undefined') {
				id = -1;
			}
			return get(apiRoot + 'SubscriptionEnumerateSubscriptions?nodeId=' + id);
		},
		sendEmail: function (id, email) {
			return get(apiRoot + 'SendEmail?nodeId=' + id + '&email='+email);
		}

	};
}]);